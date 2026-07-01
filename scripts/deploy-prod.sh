#!/usr/bin/env bash
# Production deployment entrypoint.
#
# This script is executed by the ECS self-hosted GitHub Actions runner after the
# selected production images have been built and pushed to ghcr.io.

set -euo pipefail

APP_DIR="${SRLAB_APP_DIR:-/root/home/srlab}"
DEPLOY_BRANCH="${DEPLOY_BRANCH:-master}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"
APP_COMPOSE_PROJECT="${APP_COMPOSE_PROJECT:-srlab}"
LOCK_DIR="${LOCK_DIR:-/tmp/srlab-deploy.lock}"
COMPOSE_PARALLEL_LIMIT="${COMPOSE_PARALLEL_LIMIT:-1}"
DEPLOY_STEP_TIMEOUT_SECONDS="${DEPLOY_STEP_TIMEOUT_SECONDS:-900}"
DEPLOY_LOW_IMPACT_SLEEP_SECONDS="${DEPLOY_LOW_IMPACT_SLEEP_SECONDS:-5}"
DEPLOY_LOGIN_RETRIES="${DEPLOY_LOGIN_RETRIES:-5}"
DEPLOY_LOGIN_RETRY_SLEEP_SECONDS="${DEPLOY_LOGIN_RETRY_SLEEP_SECONDS:-20}"
DEPLOY_LOGIN_TIMEOUT_SECONDS="${DEPLOY_LOGIN_TIMEOUT_SECONDS:-120}"
DEPLOY_PULL_RETRIES="${DEPLOY_PULL_RETRIES:-5}"
DEPLOY_PULL_RETRY_SLEEP_SECONDS="${DEPLOY_PULL_RETRY_SLEEP_SECONDS:-20}"
PRUNE_IMAGES="${PRUNE_IMAGES:-false}"
DEPLOY_SERVICES="${DEPLOY_SERVICES:-srlab.api srlab.ui srlab.scheduler}"
DEPLOY_STATE_DIR="${DEPLOY_STATE_DIR:-$APP_DIR/.deploy-state}"
SKIP_REPOSITORY_SYNC="${SKIP_REPOSITORY_SYNC:-false}"
DOCKER_CLIENT_TIMEOUT="${DOCKER_CLIENT_TIMEOUT:-120}"
COMPOSE_HTTP_TIMEOUT="${COMPOSE_HTTP_TIMEOUT:-120}"

export COMPOSE_PARALLEL_LIMIT
export DOCKER_CLIENT_TIMEOUT
export COMPOSE_HTTP_TIMEOUT

if [ -z "${GHCR_OWNER:-}" ]; then
    echo "GHCR_OWNER is required" >&2
    exit 1
fi

if [ -z "${IMAGE_TAG:-}" ]; then
    echo "IMAGE_TAG is required" >&2
    exit 1
fi

if ! mkdir "$LOCK_DIR" 2>/dev/null; then
    echo "Another deployment is already running: $LOCK_DIR" >&2
    exit 1
fi
trap 'rmdir "$LOCK_DIR"' EXIT

require_command() {
    if ! command -v "$1" >/dev/null 2>&1; then
        echo "Missing required command: $1" >&2
        exit 1
    fi
}

require_file() {
    if [ ! -f "$1" ]; then
        echo "Missing required production file: $1" >&2
        exit 1
    fi
}

set_env_value() {
    local key="$1"
    local value="$2"

    touch .env
    if grep -q "^${key}=" .env; then
        sed -i "s|^${key}=.*|${key}=${value}|" .env
    else
        printf '%s=%s\n' "$key" "$value" >> .env
    fi
}

get_env_value() {
    local key="$1"

    if [ ! -f .env ]; then
        return 1
    fi

    grep "^${key}=" .env | tail -1 | cut -d= -f2-
}

ensure_service_tag_defaults() {
    local legacy_tag
    legacy_tag="$(get_env_value IMAGE_TAG || true)"
    if [ -z "$legacy_tag" ]; then
        legacy_tag="latest"
    fi

    for key in API_IMAGE_TAG UI_IMAGE_TAG SCHEDULER_IMAGE_TAG; do
        if ! grep -q "^${key}=" .env 2>/dev/null; then
            set_env_value "$key" "$legacy_tag"
        fi
    done
}

service_tag_key() {
    case "$1" in
        srlab.api) echo "API_IMAGE_TAG" ;;
        srlab.ui) echo "UI_IMAGE_TAG" ;;
        srlab.scheduler) echo "SCHEDULER_IMAGE_TAG" ;;
        *)
            echo "Unknown deploy service: $1" >&2
            exit 1
            ;;
    esac
}

service_state_name() {
    case "$1" in
        srlab.api) echo "api" ;;
        srlab.ui) echo "ui" ;;
        srlab.scheduler) echo "scheduler" ;;
        *)
            echo "Unknown deploy service: $1" >&2
            exit 1
            ;;
    esac
}

normalize_deploy_services() {
    printf '%s' "$1" | tr ',;' '  ' | xargs
}

compose() {
    docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
}

has_origin_remote() {
    git config --get remote.origin.url >/dev/null 2>&1
}

sync_repository() {
    if [ -n "${GITHUB_WORKSPACE:-}" ] && [ -d "$GITHUB_WORKSPACE/.git" ] && [ "$GITHUB_WORKSPACE" != "$APP_DIR" ]; then
        echo "==> Sync repository from GitHub Actions workspace"
        git -C "$GITHUB_WORKSPACE" archive --format=tar HEAD | tar -x -C "$APP_DIR"
        return
    fi

    if [ ! -d .git ]; then
        if [ -z "${GIT_URL:-}" ]; then
            echo "$APP_DIR is not a git repository and GIT_URL is not set" >&2
            exit 1
        fi

        git init -q
        git remote add origin "$GIT_URL"
    fi

    if ! has_origin_remote; then
        if [ -z "${GIT_URL:-}" ]; then
            echo "git remote origin is missing and GIT_URL is not set" >&2
            exit 1
        fi

        git remote add origin "$GIT_URL"
    fi

    echo "==> Sync repository: origin/${DEPLOY_BRANCH}"
    git fetch origin "+refs/heads/${DEPLOY_BRANCH}:refs/remotes/origin/${DEPLOY_BRANCH}"
    git reset --hard "origin/${DEPLOY_BRANCH}"
}

print_resource_snapshot() {
    echo "==> Resource snapshot: $(date '+%Y-%m-%d %H:%M:%S')"
    docker stats --no-stream --format 'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.PIDs}}' || true
    ps -eo pid,ppid,pcpu,pmem,comm,args --sort=-pcpu | head -20 || true
    df -h / /var/lib/docker 2>/dev/null || df -h || true
}

run_compose_step() {
    local name="$1"
    shift

    echo "==> ${name}: docker compose $*"
    set +e
    if command -v timeout >/dev/null 2>&1; then
        timeout "$DEPLOY_STEP_TIMEOUT_SECONDS" docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
        local code="$?"
    else
        docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
        local code="$?"
    fi
    set -e

    if [ "$code" -ne 0 ]; then
        echo "Compose step failed: ${name} (exit ${code})" >&2
        return "$code"
    fi
}

run_compose_step_with_retry() {
    local name="$1"
    local attempts="$2"
    shift 2

    local attempt
    local code
    local sleep_seconds

    for attempt in $(seq 1 "$attempts"); do
        if run_compose_step "${name} (attempt ${attempt}/${attempts})" "$@"; then
            return 0
        fi

        code="$?"
        if [ "$attempt" -ge "$attempts" ]; then
            echo "Step failed after ${attempts} attempts: ${name}" >&2
            return "$code"
        fi

        sleep_seconds=$((DEPLOY_PULL_RETRY_SLEEP_SECONDS * attempt))
        echo "Retrying ${name} in ${sleep_seconds}s"
        sleep "$sleep_seconds"
    done
}

login_ghcr_with_retry() {
    local attempt
    local code
    local sleep_seconds

    for attempt in $(seq 1 "$DEPLOY_LOGIN_RETRIES"); do
        echo "==> Login to ghcr.io (attempt ${attempt}/${DEPLOY_LOGIN_RETRIES})"
        set +e
        if command -v timeout >/dev/null 2>&1; then
            timeout "$DEPLOY_LOGIN_TIMEOUT_SECONDS" docker login ghcr.io -u "$GHCR_USERNAME" --password-stdin <<< "$GHCR_TOKEN"
            code="$?"
        else
            docker login ghcr.io -u "$GHCR_USERNAME" --password-stdin <<< "$GHCR_TOKEN"
            code="$?"
        fi
        set -e

        if [ "$code" -eq 0 ]; then
            return 0
        fi

        if [ "$attempt" -ge "$DEPLOY_LOGIN_RETRIES" ]; then
            echo "Login to ghcr.io failed after ${DEPLOY_LOGIN_RETRIES} attempts" >&2
            return "$code"
        fi

        sleep_seconds=$((DEPLOY_LOGIN_RETRY_SLEEP_SECONDS * attempt))
        echo "Retrying ghcr.io login in ${sleep_seconds}s"
        sleep "$sleep_seconds"
    done
}

record_deploy_state() {
    local deploy_sha="${GITHUB_SHA:-}"
    if [ -z "$deploy_sha" ]; then
        deploy_sha="$(git rev-parse HEAD 2>/dev/null || true)"
    fi

    if [ -z "$deploy_sha" ]; then
        echo "==> Skip deploy state update: deploy SHA is unknown"
        return
    fi

    mkdir -p "$DEPLOY_STATE_DIR"
    for service in $DEPLOY_SERVICES; do
        local state_name
        state_name="$(service_state_name "$service")"
        printf '%s\n' "$deploy_sha" > "$DEPLOY_STATE_DIR/${state_name}.sha"
    done
    printf '%s\n' "$deploy_sha" > "$DEPLOY_STATE_DIR/compose.sha"
    echo "==> Updated deploy state: $deploy_sha"
}

require_command git
require_command docker

if ! docker compose version >/dev/null 2>&1; then
    echo "docker compose v2 is required" >&2
    exit 1
fi

if [ ! -d "$APP_DIR" ]; then
    echo "Application directory does not exist: $APP_DIR" >&2
    exit 1
fi

cd "$APP_DIR"

if [ "$SKIP_REPOSITORY_SYNC" = "true" ]; then
    echo "==> Skip repository sync"
else
    sync_repository
fi

require_file "$COMPOSE_FILE"
require_file "SpeedRunners.API/SpeedRunners/appsettings.Production.json"
require_file "SpeedRunners.Scheduler/App.config"

DEPLOY_SERVICES="$(normalize_deploy_services "$DEPLOY_SERVICES")"
if [ -z "$DEPLOY_SERVICES" ]; then
    echo "No services selected for deployment"
    exit 0
fi

ensure_service_tag_defaults
set_env_value GHCR_OWNER "$GHCR_OWNER"
set_env_value IMAGE_TAG "$IMAGE_TAG"
for service in $DEPLOY_SERVICES; do
    tag_key="$(service_tag_key "$service")"
    set_env_value "$tag_key" "$IMAGE_TAG"
done

if [ -n "${GHCR_USERNAME:-}" ] && [ -n "${GHCR_TOKEN:-}" ]; then
    login_ghcr_with_retry
fi

echo "==> Deploy images: owner=${GHCR_OWNER} tag=${IMAGE_TAG}"
echo "==> Compose project: ${APP_COMPOSE_PROJECT}, parallel limit: ${COMPOSE_PARALLEL_LIMIT}"
echo "==> Login retries: ${DEPLOY_LOGIN_RETRIES}, login timeout: ${DEPLOY_LOGIN_TIMEOUT_SECONDS}s"
echo "==> Pull retries: ${DEPLOY_PULL_RETRIES}, retry base sleep: ${DEPLOY_PULL_RETRY_SLEEP_SECONDS}s"
print_resource_snapshot

run_compose_step "Ensure mysql is running" up -d srlab.mysql

for service in $DEPLOY_SERVICES; do
    run_compose_step_with_retry "Pull ${service}" "$DEPLOY_PULL_RETRIES" pull "$service"
    print_resource_snapshot

    run_compose_step "Start ${service}" up -d --no-deps "$service"
    print_resource_snapshot

    sleep "$DEPLOY_LOW_IMPACT_SLEEP_SECONDS"
done

if [ "$PRUNE_IMAGES" = "true" ]; then
    echo "==> Prune dangling images"
    docker image prune -f
else
    echo "==> Skip image prune (set PRUNE_IMAGES=true to enable)"
fi

echo "==> Container status"
compose ps
record_deploy_state

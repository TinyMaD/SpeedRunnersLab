#!/usr/bin/env bash
# Production deployment entrypoint.
#
# This script is executed by the ECS self-hosted GitHub Actions runner after the
# three production images have been built and pushed to ghcr.io.

set -euo pipefail

APP_DIR="${SRLAB_APP_DIR:-/root/home/srlab}"
DEPLOY_BRANCH="${DEPLOY_BRANCH:-master}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"
APP_COMPOSE_PROJECT="${APP_COMPOSE_PROJECT:-srlab}"
LOCK_DIR="${LOCK_DIR:-/tmp/srlab-deploy.lock}"
COMPOSE_PARALLEL_LIMIT="${COMPOSE_PARALLEL_LIMIT:-1}"
DEPLOY_STEP_TIMEOUT_SECONDS="${DEPLOY_STEP_TIMEOUT_SECONDS:-900}"
DEPLOY_LOW_IMPACT_SLEEP_SECONDS="${DEPLOY_LOW_IMPACT_SLEEP_SECONDS:-5}"
PRUNE_IMAGES="${PRUNE_IMAGES:-false}"
DEPLOY_SERVICES="${DEPLOY_SERVICES:-srlab.api srlab.ui srlab.scheduler}"

export COMPOSE_PARALLEL_LIMIT

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

compose() {
    docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
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
    if command -v timeout >/dev/null 2>&1; then
        timeout "$DEPLOY_STEP_TIMEOUT_SECONDS" docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
    else
        docker compose -p "$APP_COMPOSE_PROJECT" -f "$COMPOSE_FILE" "$@"
    fi
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

if [ ! -d .git ]; then
    if [ -z "${GIT_URL:-}" ]; then
        echo "$APP_DIR is not a git repository and GIT_URL is not set" >&2
        exit 1
    fi

    git init -q
    git remote add origin "$GIT_URL"
fi

if ! git remote get-url origin >/dev/null 2>&1; then
    if [ -z "${GIT_URL:-}" ]; then
        echo "git remote origin is missing and GIT_URL is not set" >&2
        exit 1
    fi

    git remote add origin "$GIT_URL"
fi

echo "==> Sync repository: origin/${DEPLOY_BRANCH}"
git fetch origin "+refs/heads/${DEPLOY_BRANCH}:refs/remotes/origin/${DEPLOY_BRANCH}"
git reset --hard "origin/${DEPLOY_BRANCH}"

require_file "$COMPOSE_FILE"
require_file "SpeedRunners.API/SpeedRunners/appsettings.Production.json"
require_file "SpeedRunners.Scheduler/App.config"

set_env_value GHCR_OWNER "$GHCR_OWNER"
set_env_value IMAGE_TAG "$IMAGE_TAG"

if [ -n "${GHCR_USERNAME:-}" ] && [ -n "${GHCR_TOKEN:-}" ]; then
    echo "==> Login to ghcr.io"
    printf '%s' "$GHCR_TOKEN" | docker login ghcr.io -u "$GHCR_USERNAME" --password-stdin
fi

echo "==> Deploy images: owner=${GHCR_OWNER} tag=${IMAGE_TAG}"
echo "==> Compose project: ${APP_COMPOSE_PROJECT}, parallel limit: ${COMPOSE_PARALLEL_LIMIT}"
print_resource_snapshot

run_compose_step "Ensure mysql is running" up -d srlab.mysql

for service in $DEPLOY_SERVICES; do
    run_compose_step "Pull ${service}" pull "$service"
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

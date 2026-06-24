#!/usr/bin/env bash
# Detect which production services need a new image/deployment.

set -euo pipefail

APP_DIR="${SRLAB_APP_DIR:-/root/home/srlab}"
STATE_DIR="${DEPLOY_STATE_DIR:-$APP_DIR/.deploy-state}"
SELECTION="${DEPLOY_SERVICE_SELECTION:-auto}"
HEAD_SHA="${GITHUB_SHA:-$(git rev-parse HEAD)}"

build_api=false
build_ui=false
build_scheduler=false
deploy_services=()

ensure_commit_available() {
    local sha="$1"
    local attempt

    if git cat-file -e "$sha^{commit}" 2>/dev/null; then
        return 0
    fi

    for attempt in 1 2 3; do
        echo "Fetching comparison commit ${sha} (attempt ${attempt})"
        if git fetch --no-tags --depth=1 origin "$sha" >/dev/null 2>&1; then
            git cat-file -e "$sha^{commit}" 2>/dev/null && return 0
        fi
        sleep $((attempt * 3))
    done

    return 1
}

normalize_selection() {
    printf '%s' "$1" \
        | tr '[:upper:]' '[:lower:]' \
        | tr ',;' '  ' \
        | xargs
}

add_deploy_service() {
    local service="$1"
    local existing
    for existing in "${deploy_services[@]}"; do
        if [ "$existing" = "$service" ]; then
            return
        fi
    done
    deploy_services+=("$service")
}

state_env_name() {
    case "$1" in
        api) echo "DEPLOY_STATE_API_SHA" ;;
        ui) echo "DEPLOY_STATE_UI_SHA" ;;
        scheduler) echo "DEPLOY_STATE_SCHEDULER_SHA" ;;
        compose) echo "DEPLOY_STATE_COMPOSE_SHA" ;;
        *)
            echo "Unknown deploy state: $1" >&2
            exit 1
            ;;
    esac
}

get_state_sha() {
    local state_name="$1"
    local env_name
    env_name="$(state_env_name "$state_name")"

    if [ -n "${!env_name:-}" ]; then
        printf '%s' "${!env_name}" | tr -d '[:space:]'
        return
    fi

    local state_file="$STATE_DIR/$state_name.sha"
    if [ -f "$state_file" ]; then
        tr -d '[:space:]' < "$state_file"
    fi
}

set_service_selected() {
    case "$1" in
        api)
            build_api=true
            add_deploy_service "srlab.api"
            ;;
        ui)
            build_ui=true
            add_deploy_service "srlab.ui"
            ;;
        scheduler)
            build_scheduler=true
            add_deploy_service "srlab.scheduler"
            ;;
        *)
            echo "Unknown service selection: $1" >&2
            exit 1
            ;;
    esac
}

changed_since_service_deploy() {
    local state_name="$1"
    shift

    local base_sha
    base_sha="$(get_state_sha "$state_name")"
    if [ -z "$base_sha" ]; then
        return 0
    fi

    if ! ensure_commit_available "$base_sha"; then
        echo "Comparison commit ${base_sha} is unavailable; selecting ${state_name} for a safe deployment"
        return 0
    fi

    if [ "$base_sha" = "$HEAD_SHA" ]; then
        return 1
    fi

    if git diff --quiet "$base_sha..$HEAD_SHA" -- "$@"; then
        return 1
    fi

    return 0
}

SELECTION="$(normalize_selection "$SELECTION")"
if [ -z "$SELECTION" ] || [ "$SELECTION" = "auto" ]; then
    if changed_since_service_deploy api "SpeedRunners.API"; then
        build_api=true
        add_deploy_service "srlab.api"
    fi

    if changed_since_service_deploy ui "SpeedRunners.UI"; then
        build_ui=true
        add_deploy_service "srlab.ui"
    fi

    if changed_since_service_deploy scheduler "SpeedRunners.Scheduler"; then
        build_scheduler=true
        add_deploy_service "srlab.scheduler"
    fi

    if changed_since_service_deploy compose "docker-compose.prod.yml"; then
        build_api=true
        build_ui=true
        build_scheduler=true
        add_deploy_service "srlab.api"
        add_deploy_service "srlab.ui"
        add_deploy_service "srlab.scheduler"
    fi
elif [ "$SELECTION" = "all" ]; then
    set_service_selected api
    set_service_selected ui
    set_service_selected scheduler
else
    for service in $SELECTION; do
        set_service_selected "$service"
    done
fi

deploy_services_text="${deploy_services[*]:-}"

echo "build_api=$build_api" >> "$GITHUB_OUTPUT"
echo "build_ui=$build_ui" >> "$GITHUB_OUTPUT"
echo "build_scheduler=$build_scheduler" >> "$GITHUB_OUTPUT"
echo "deploy_services=$deploy_services_text" >> "$GITHUB_OUTPUT"

echo "Detected build_api=$build_api"
echo "Detected build_ui=$build_ui"
echo "Detected build_scheduler=$build_scheduler"
echo "Detected deploy_services=${deploy_services_text:-<none>}"

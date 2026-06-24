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

    local state_file="$STATE_DIR/$state_name.sha"
    if [ ! -f "$state_file" ]; then
        return 0
    fi

    local base_sha
    base_sha="$(tr -d '[:space:]' < "$state_file")"
    if [ -z "$base_sha" ]; then
        return 0
    fi

    if ! git cat-file -e "$base_sha^{commit}" 2>/dev/null; then
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

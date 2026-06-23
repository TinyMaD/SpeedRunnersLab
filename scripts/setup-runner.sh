#!/usr/bin/env bash
# One-time setup for the production GitHub Actions self-hosted runner.

set -euo pipefail

APP_DIR="${SRLAB_APP_DIR:-/root/home/srlab}"
ENV_FILE="${GITHUB_RUNNER_ENV_FILE:-/root/srlab-runner.env}"
DATA_DIR="${GITHUB_RUNNER_DATA_DIR:-/root/srlab-runner-data}"
REPO_URL="${GITHUB_RUNNER_REPO_URL:-https://github.com/TinyMaD/SpeedRunnersLab}"
RUNNER_NAME="${GITHUB_RUNNER_NAME:-speedrunners-prod}"
RUNNER_LABELS="${GITHUB_RUNNER_LABELS:-speedrunners-prod}"

C_RESET="\033[0m"; C_CYAN="\033[36m"; C_GREEN="\033[32m"; C_YELLOW="\033[33m"; C_RED="\033[31m"
step() { echo; echo -e "${C_CYAN}==> $1${C_RESET}"; }
ok()   { echo -e "  ${C_GREEN}[OK]${C_RESET} $1"; }
warn() { echo -e "  ${C_YELLOW}[!]${C_RESET} $1"; }
err()  { echo -e "  ${C_RED}[X]${C_RESET} $1" >&2; }

if [ "$(id -u)" -ne 0 ]; then
    err "Please run as root on ECS."
    exit 1
fi

step "Check prerequisites"
if ! command -v docker >/dev/null 2>&1; then
    err "docker is not installed"
    exit 1
fi
ok "docker is installed"

if ! docker compose version >/dev/null 2>&1; then
    err "docker compose v2 is required"
    exit 1
fi
ok "$(docker compose version | head -1)"

if [ ! -f "$APP_DIR/docker-compose.runner.yml" ]; then
    err "Missing $APP_DIR/docker-compose.runner.yml. Run this after pulling the latest repository on ECS."
    exit 1
fi
ok "found docker-compose.runner.yml"

step "Collect runner registration token"
if [ -f "$ENV_FILE" ]; then
    # shellcheck disable=SC1090
    source "$ENV_FILE"
fi

if [ -z "${GITHUB_RUNNER_PAT:-}" ]; then
    echo "Create a fine-grained GitHub PAT with repository Administration: Read and write."
    echo "Repository: $REPO_URL"
    read -r -s -p "GitHub PAT: " GITHUB_RUNNER_PAT
    echo
fi

if [ -z "$GITHUB_RUNNER_PAT" ]; then
    err "GITHUB_RUNNER_PAT is required"
    exit 1
fi

step "Write runner environment"
umask 077
cat > "$ENV_FILE" <<EOF
GITHUB_RUNNER_PAT=$GITHUB_RUNNER_PAT
GITHUB_RUNNER_REPO_URL=$REPO_URL
GITHUB_RUNNER_NAME=$RUNNER_NAME
GITHUB_RUNNER_LABELS=$RUNNER_LABELS
GITHUB_RUNNER_DATA_DIR=$DATA_DIR
SRLAB_APP_DIR=$APP_DIR
EOF
chmod 600 "$ENV_FILE"
ok "wrote $ENV_FILE"

mkdir -p "$DATA_DIR/work" "$DATA_DIR/config"
ok "runner data directory: $DATA_DIR"

step "Start runner"
cd "$APP_DIR"
docker compose -p srlab-runner --env-file "$ENV_FILE" -f docker-compose.runner.yml up -d
ok "runner container started"

echo
echo "Useful commands:"
echo "  docker ps --filter name=srlab-github-runner"
echo "  docker logs --tail 100 srlab-github-runner"
echo
echo "GitHub runner label used by deploy.yml: $RUNNER_LABELS"

# Steps:
#   1. Check the requested tag is valid
#   2. Sync config files (docker-compose.yml, this script) with GitHub main
#   3. Pull the new backend/frontend images and restart them
#   4. Wait until the app answers on /api/health
#
set -euo pipefail   # strict mode: stop on errors, unset variables, and failed pipes

# Settings ────────────────────────────────────────────────────────────────
HEALTH_URL="http://localhost/api/health"   
HEALTH_ATTEMPTS=30                         
HEALTH_WAIT_SECONDS=2                      
APP_SERVICES="backend frontend"            

# Helpers ─────────────────────────────────────────────────────────────────

step() {
  echo
  echo "==> $*"
}

fail() {
  echo "ERROR: $*" >&2
  exit 1
}

# docker compose, always with both env files:
#   .env         → passwords 
#   .deploy.env  → IMAGE_TAG (which version to run)
compose() {
  docker compose --env-file .env --env-file .deploy.env "$@"
}

# Steps ───────────────────────────────────────────────────────────────────

go_to_app_folder() {
  # app folder is one level above this script
  local script_folder
  script_folder="$(dirname "$0")"
  cd "$script_folder/.."
}

read_requested_tag() {
  # CI passes the tag through SSH; a human passes it as the first argument.
  if [[ -n "${SSH_ORIGINAL_COMMAND:-}" ]]; then
    echo "$SSH_ORIGINAL_COMMAND"
  else
    echo "${1:-}"
  fi
}

validate_tag() {
  local tag="$1"
  # Allowed  latest   or   sha-<7 to 40 hex characters>
  if [[ ! "$tag" =~ ^(latest|sha-[0-9a-f]{7,40})$ ]]; then
    fail "Invalid tag '$tag'. Expected 'latest' or 'sha-<commit hash>'."
  fi
}

sync_repo_with_github() {
  # Make the files match GitHub main
  # Untracked/ignored files (.env, init.sql, postgres-data/) are never touched
  git fetch --quiet origin main
  git reset --hard --quiet origin/main
}

deploy_images() {
  local tag="$1"
  echo "IMAGE_TAG=$tag" > .deploy.env      # remember what's running
  compose pull $APP_SERVICES               # download new images
  compose up -d --no-build --remove-orphans  # restart only what changed
}

wait_until_healthy() {
  for attempt in $(seq 1 "$HEALTH_ATTEMPTS"); do
    if curl --fail --silent --show-error "$HEALTH_URL" > /dev/null; then
      echo "App is healthy (after $((attempt * HEALTH_WAIT_SECONDS))s)."
      return 0
    fi
    echo "Not ready yet (attempt $attempt/$HEALTH_ATTEMPTS)..."
    sleep "$HEALTH_WAIT_SECONDS"
  done

  echo "Last 50 backend log lines:" >&2
  compose logs --tail 50 backend >&2
  fail "App did not become healthy in time."
}

clean_up_old_images() {
  docker image prune --force > /dev/null   # delete unused old versions 
}

# Main ───────────────────────────────────
main() {
  go_to_app_folder

  local tag
  tag="$(read_requested_tag "$@")"

  step "[1/4] Checking tag"
  validate_tag "$tag"
  echo "Deploying: $tag"

  step "[2/4] Syncing repo with GitHub main"
  sync_repo_with_github

  step "[3/4] Pulling and starting new images"
  deploy_images "$tag"

  step "[4/4] Health check"
  wait_until_healthy

  clean_up_old_images
  echo
  echo "Deploy of $tag finished successfully."
}

# Run main then exit immediately
main "$@"; exit
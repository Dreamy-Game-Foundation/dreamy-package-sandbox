#!/usr/bin/env bash

set -u

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
PACKAGE_ROOT="${SCRIPT_DIR}/../Packages"
ORG="https://github.com/Dreamy-Game-Foundation"

PACKAGES=(
  com.dreamy.core
  com.dreamy.ui
  com.dreamy.audio
  com.dreamy.assets
  com.dreamy.editor-tools
  com.dreamy.datasave
  com.dreamy.dataconfig
  com.dreamy.feedback
  com.dreamy.localization
)

mkdir -p "${PACKAGE_ROOT}"

echo "Package source root: ${PACKAGE_ROOT}"
echo

for package in "${PACKAGES[@]}"; do
  destination="${PACKAGE_ROOT}/${package}"

  if [ -d "${destination}/.git" ]; then
    echo "[exists] ${package}"
    git -C "${destination}" status --short --branch || true
    continue
  fi

  if [ -e "${destination}" ]; then
    echo "[blocked] ${package}: path exists but is not a Git repository; nothing changed"
    continue
  fi

  echo "[clone] ${package}"
  if ! git clone "${ORG}/${package}.git" "${destination}"; then
    echo "[failed] ${package}: clone failed; continuing with remaining packages"
    continue
  fi

  git -C "${destination}" status --short --branch || true
done

echo
echo "Setup complete. No existing repository was reset or discarded."

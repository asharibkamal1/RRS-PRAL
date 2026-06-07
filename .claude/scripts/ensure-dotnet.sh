#!/usr/bin/env bash
# Ensures the .NET 10 SDK is available in this (ephemeral) web-session container.
# The project targets net10.0; Ubuntu 24.04 'noble-updates/universe' ships dotnet-sdk-10.0,
# and archive.ubuntu.com is reachable under the network policy, so apt is the reliable source.
set -euo pipefail

if command -v dotnet >/dev/null 2>&1; then
    exit 0
fi

SUDO=""
if [ "$(id -u)" -ne 0 ] && command -v sudo >/dev/null 2>&1; then
    SUDO="sudo"
fi

export DEBIAN_FRONTEND=noninteractive
$SUDO apt-get update -qq || true
$SUDO apt-get install -y -qq dotnet-sdk-10.0

command -v dotnet >/dev/null 2>&1 && echo "dotnet $(dotnet --version) ready"

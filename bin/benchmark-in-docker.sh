#!/usr/bin/env bash

# Synopsis:
# Benchmark the test runner Docker image.
# Each run will clean the test directory being used to prevent previous runs from affecting the results.

# Environment:
# SKIP_DOCKER_BUILD: if set, skip building the Docker image

# Output:
# Outputs performance metrics.

# Example:
# ./bin/benchmark-in-docker.sh

set -eo pipefail

die() { echo "$*" >&2; exit 1; }

required_tool() {
    command -v "${1}" >/dev/null 2>&1 ||
        die "${1} is required but not installed. Please install it and make sure it's in your PATH."
}

required_tool docker
required_tool hyperfine

# Pre-build the Docker image
if [ -z "${SKIP_DOCKER_BUILD}" ]; then
  docker build --rm -t exercism/csharp-test-runner .
else
  echo "Skipping docker build because SKIP_DOCKER_BUILD is set."
fi

hyperfine \
    --parameter-list slug $(find tests -maxdepth 1 -mindepth 1 -type d -printf $'%f\n' | paste -sd ",") \
    --prepare 'git clean -xdfq tests/{slug}' \
    'SKIP_DOCKER_BUILD=true bin/run-in-docker.sh {slug} tests/{slug} tests/{slug}'

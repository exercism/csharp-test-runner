#!/bin/sh
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

pwsh /opt/test-runner/bin/run.ps1 $1 $2 $3
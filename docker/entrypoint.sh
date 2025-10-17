#!/bin/sh
set -e

echo "Starting TasksManager API..."

exec dotnet TasksManager.Api.dll

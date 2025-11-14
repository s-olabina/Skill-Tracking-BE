#!/bin/bash
set -e

echo "Running EF Core migrations..."

dotnet tool install --global dotnet-ef --version 8.0.10

export PATH="$PATH:/root/.dotnet/tools"

dotnet ef database update \
    --project SkillTrackingApp.csproj \
    --startup-project SkillTrackingApp.csproj

echo "Starting app..."
exec dotnet run
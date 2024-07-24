#!/usr/bin/env bash

set -e

dotnet pack ./Belkonar.GitHubAppHelper -o out -c Release /p:Version=$1
dotnet nuget push ./out/Belkonar.GitHubAppHelper.$1.nupkg --api-key $NUGET_TOKEN --source https://api.nuget.org/v3/index.json

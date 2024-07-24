#!/usr/bin/env bash

set -e

cd Belkonar.GitHubAppHelper/
dotnet pack -o out -c Release /p:Version=$1
dotnet nuget push out/Belkonar.GitHubAppHelper.$1.nupkg --api-key $NUGET_TOKEN --source https://api.nuget.org/v3/index.json

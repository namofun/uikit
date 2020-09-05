#!/bin/bash
NUGET_KEY=$1
dotnet pack -c Release -o packages
dotnet nuget push -s https://nuget.xylab.fun/v3/index.json -k $NUGET_KEY packages/*.nupkg

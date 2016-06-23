#!/usr/bin/env bash

# ref http://andrewlock.net/adding-travis-ci-to-a-net-core-app/
#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

dotnet restore
 
#dotnet test ./src/Host -c Debug -f netcoreapp1.0
dotnet build ./src/Host -c Debug -f netcoreapp1.0

#dotnet test ./src/RigoFunc.IdentityServer -c Debug -f netcoreapp1.0
dotnet build ./src/RigoFunc.IdentityServer Host -c Debug -f netcoreapp1.0


revision=${TRAVIS_JOB_ID:=1}  
revision=$(printf "%04d" $revision) 

#dotnet pack ./src/RigoFunc.IdentityServer -c Debug -o ./artifacts --version-suffix=$revision  
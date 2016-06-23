#!/usr/bin/env bash

# ref http://andrewlock.net/adding-travis-ci-to-a-net-core-app/
#exit if any command fails
set -e

wget http://download.microsoft.com/download/C/3/F/C3FB57E8-03EB-4D82-946D-C0FD9864450C/dotnet-ubuntu-x64.1.0.0-rc2-3002702.tar.gz
tar -zxvf dotnet-ubuntu-x64.1.0.0-rc2-3002702.tar.gz

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

./dotnet restore
 
#dotnet test ./src/Host -c Debug -f netcoreapp1.0
./dotnet build ./src/Host -c Debug -f netcoreapp1.0

#dotnet test ./src/RigoFunc.IdentityServer -c Debug -f netcoreapp1.0
./dotnet build ./src/RigoFunc.IdentityServer Host -c Debug -f netcoreapp1.0


revision=${TRAVIS_JOB_ID:=1}  
revision=$(printf "%04d" $revision) 

#dotnet pack ./src/RigoFunc.IdentityServer -c Debug -o ./artifacts --version-suffix=$revision  
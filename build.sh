#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

dotnet restore

dotnet build src/Pickles.ObjectModel
dotnet build src/Pickles.TestFrameworks
dotnet build src/Pickles
dotnet build src/Pickles.CommandLine

revision=${TRAVIS_JOB_ID:=1}  
revision=$(printf "%04d" $revision) 

#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

dotnet restore

dotnet build Pickles.ObjectModel
dotnet build Pickles.TestFrameworks
dotnet build Pickles
dotnet build Pickles.CommandLine

revision=${TRAVIS_JOB_ID:=1}  
revision=$(printf "%04d" $revision) 

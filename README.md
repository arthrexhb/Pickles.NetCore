# Pickles.NetCore 

|              |                                                                                                                           |
|--------------|---------------------------------------------------------------------------------------------------------------------------|
| Linux/OSX    | [![Build Status](https://travis-ci.org/ngm/Pickles.NetCore.svg?branch=master)](https://travis-ci.org/ngm/Pickles.NetCore) |
| Windows      | [![Build status](https://ci.appveyor.com/api/projects/status/p0tq7uq0nxoraf2c?svg=true)](https://ci.appveyor.com/project/ngm/pickles-netcore) 

This is a proof-of-concept to investigate porting [Pickles](https://github.com/picklesdoc/pickles) to cross-platform .NET Core.

It should be considered quite experimental at present, and is quite out of sync with the main Pickles repository.  It does, however, work (to some degree).  Please see https://github.com/picklesdoc/pickles/issues/235 for further discussion.

I have built custom .NET Core versions of some dependent projects.  These are published at  https://www.myget.org/F/doubleloop/api/v3/index.json.  That myget source is included via the local NuGet.config in this repository.

## Running

Example:

    dotnet run --f /home/user/Code/Project/Features --o /home/user/Code/Projcet/Output/ --df dhtml

#!/bin/bash

set -ev

rm -r Cli/bin Cli/obj
dotnet build
dotnet publish Cli -c Release -r linux-x64
warp-packer --arch linux-x64 --input_dir Cli/bin/Release/netcoreapp2.2/linux-x64/publish --exec Cli --output Cli/bin/Release/photos-organizer
dotnet build

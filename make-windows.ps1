rm -r -fo Cli/bin
rm -r -fo Cli/obj

dotnet build
dotnet publish Cli -c Release -r win-x64
warp-packer --arch windows-x64 --input_dir Cli/bin/Release/net5/win-x64/publish --exec Cli.exe --output Cli/bin/Release/photos-organizer.exe
dotnet build

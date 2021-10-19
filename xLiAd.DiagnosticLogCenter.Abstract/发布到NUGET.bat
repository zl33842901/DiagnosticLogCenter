del nupkgs\* /q
del nupkgsSymb\* /q
msbuild.exe   /property:Configuration=Debug
msbuild.exe   /property:Configuration=Release
dotnet pack --no-build --output nupkgs --include-symbols
move nupkgs\*.symbols.nupkg nupkgsSymb\
D:\nuget push .\nupkgsSymb\* -Source https://api.nuget.org/v3/index.json
D:\nuget push .\nupkgsSymb\* HjwJmzXbJyzZlgZwLMh -Source http://nuget.cig.com.cn/nuget
pause
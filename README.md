# Welcome to the DLinter Project

DLint is a prototype for Dockerfile Linter, capable of doing static code analysis on dockerfiles.
The linter scans for a number of common misconfigurations and tries to uphold Dockerfile best practices.


## Overview
1. [Downloading](#downloading)
4. [Prerequisites](#prerequisites)
5. [Building the Project](#building-the-project)
8. [Running the project](#running-the-project)
10. [Logging Mode](#set-csv-logging-mode)
   
### Notice: Exec format error
DLint depends on the shellcheck binary. It ships with the one for macOS. If running into an **'Exec format error'**, replace the binary in Shellcheck/shellcheck with the one compatible to your system. 
```
# On Linux you can simply:
apt-get install shellcheck
cp /usr/bin/shellcheck ./Shellcheck/shellcheck
```
For Windows based systems you can download pre compiled binaries from: https://github.com/koalaman/shellcheck?tab=readme-ov-file#installing in the bottom of the installation section, and replace the .\hellcheck\shellcheck file with the downloaded one.


# Downloading
## For Production Use
Although DLint offers valuable insights into misconfigurations and vulnerabilities, it is only a prototype! DLint is not currently intended for prodution use, but we encourage you to clone or fork this repository and extend it.

## For Testing and Development
For testing and development, having a local copy of the git repository with the entire project history gives you much more insight into the code base.
A local copy of the Git Repository can be obtained by cloning it from this original repository using
```
git clone https://github.com/LysetsDal/DLint.git
```

# Prerequisites
DLint needs **dotnet 6.0** (or newer) to build and run the application.


# Building the Project
DLint was developed on a macOS system. In the DLint folder is a Dockerfile that we can use to test.


## On MacOS (arm64 / aarch64)
From the DLint/DLint folder (the one with the .fsproj file in it) do:
```
dotnet clean && dotnet restore;
dotnet publish -c Release -r osx-arm64 
```
Binary compiled to: ``DLint/DLint/bin/Release/net6.0/osx-arm64``
</br>
</br>
## On Windows
To compile the program on windows, choose architecture: 
- win-x64 
- win-x86

For full list see: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
```
dotnet clean && dotnet restore;
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
```
Binary compiled to: ``DLint/DLint/bin/Release/net6.0/osx-arm64``
</br>


# Running the project
If dotnet is installed you can simply use this command:
```
dotnet run Dockerfile
```
Dockerfile = path to the dockerfile you want to run (relative to the project folder) </br>
*NOTE:* DLint can take multiple Dockerfiles. Just separate them by a singel space:
```
dotnet run ./Dockerfile1 ./backend/Dockerfile2
```

Or use the compiled binary: 
```
chmod +x ./bin/Release/net6.0/osx-arm64/DLint &&
./bin/Release/net6.0/osx-arm64/DLint Dockerfile
```
Note: the dockerfile paths needs to be relative to your current directory.
</br>
</br>

## Run all test Dockerfiles
```
ls -l ../resources | awk 'NR > 1 {print $9}'
| xargs -I {} dotnet run ../resources/{} \
| awk '{gsub(/File Read:/, "\033[1;31m&\033[0m"); print}'
```
Note:
- First line gets the filenames of the folder with the Dockerfiles. 
- Second command passes the filename to run sequentially through xargs.
- Third line highlights 'File Read' in read, each time a new file is scanned.
</br>
</br>

## Set CSV-Logging Mode
DLint supports two flags:
```
dotnet run [ --log-mode=csv || --log-mode=normal ] <Dockerfile_path>
```
The 'normal' log mode is the default.
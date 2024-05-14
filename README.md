# Welcome to the DLinter Project

DLint is a prototype for Dockerfile Linter, capable of doing static code analysis on dockerfiles.
The linter scans for a number of common misconfigurations and tries to uphold Dockerfile best practices.


## Overview
1. [Downloading](#downloading)
   2. [For Production Use](#for-production-use)
   3. [For Testing and Development](#for-testing-and-development)
4. [Prerequisites](#prerequisites)
5. [Building the Project](#building-the-project)
   6. [Mac](#on-macos-(arm64-/-aarch64))
   7. [Windows](#on-windows)
8. [Running the project](#running-the-project)
   9. [Run all test Dockerfiles](#run-all-test-dockerfiles)
   10. [Logging Mode](#set-csv-logging-mode)
   


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


## On Windows
To compile the program on windows, choose architecture: 
- win-x64 
- win-x86

For full list see: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
```
dotnet clean && dotnet restore;
dotnet publish -c Release -r <insert_architecture>
```
Binary compiled to: ``DLint/DLint/bin/Release/net6.0/osx-arm64``


# Running the project
If dotnet is installed you can simply use this command:
```
dotnet run Dockerfile
```
Dockerfile = path to the dockerfile you want to run (relative to the project folder)
*NOTE:* DLint can take multiple Dockerfiles. Just separate them bu a singel space:
```
dotnet run ./Dockerfile1 ./backend/Dockerfile2
```

Or use the compiled binary: 
```
chmod +x ./bin/Release/net6.0/osx-arm64/DLint &&
./bin/Release/net6.0/osx-arm64/DLint Dockerfile
```
Note: the dockerfile paths needs to be relative to your current directory.

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

## Set CSV-Logging Mode
DLint supports two flags:
```
dotnet run [ --log-mode=csv || --log-mode=normal ] <Dockerfile_path>
```
The 'normal' log mode is the default.
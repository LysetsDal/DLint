# Welcome to the Linterd Project

Linterd is a prototype for Dockerfile Linter, capable of doing static code analysis on dockerfiles.
The linter scans for a number of common misconfigurations and tries to uphold Dockerfile best practices.


## Overview 


# Downloading
## For Production Use
Although Linterd offers valuable insights into misconfigurations and vulnerabilities, it is only a prototype! Linterd is not currently intended for prodution use, but encourage you to clone or fork this repository and extend it.

## For Testing and Development
For testing and development, having a local copy of the git repository with the entire project history gives you much more insight into the code base.
A local copy of the Git Repository can be obtained by cloning it from this original repository using
```
git clone https://github.com/LysetsDal/Linterd.git
```

# Prerequisites
Linterd needs **dotnet 6.0** to build and run the application.

To run the Lexer and Parser generator, you need mono installed:
```
brew install mono 
```
***

# Build and Install
## Generating the lexer and parser for Linterd:
### On macOS:

From the src directory run this command in a shell:
````
mono $fslex --unicode DLex.fsl ;\
mono $fsyacc --module DPar DPar.fsy ;\
fsharpi -r /etc/fsharp/FsLexYacc.Runtime.dll Absyn.fs Utils.fs DPar.fs \
DLex.fs Config.fs Interp.fs Program.fs

````

Now open the Program module and run the main function like this:
```
open Program
Main <args>

//Example: 
Main Dockerfile1 Dockerfile2
```

### Building the project:
From the Linterd folder (with .fsproj in it) do:
```
dotnet clean && dotnet restore;
dotnet build 
```
Binary compiled to: ``Linterd/bin/Debug/net6.0/Linterd``

## Running the project:
From the Linterd folder do
```
dotnet run <Dockerfile_path1> <Dockerfile_path2> ... 
```
Or using the binary: 
```
chmod +x ./bin/Debug/net6.0/Linterd &&
./bin/Debug/net6.0/Linterd <Dockerfile_path> <Dockerfile_path2> ...
```
Note: the dockerfile paths needs to be relative to your current directory.
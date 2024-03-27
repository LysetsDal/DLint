# Linterd

Prototype for Dockerfile Linter

## Generating the lexer and parser for Linterd:

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

## Building the project:
From the root folder: Linterd do
```
dotnet clean && dotnet restore;
dotnet build 
```
Binary compiled to: ``Linterd/src/bin/Debug/net6.0/Linterd``

## Running the project:
From the root folder: Linterd run
```
dotnet run <Dockerfile1> 
```
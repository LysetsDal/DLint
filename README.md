# Linterd

Prototype for Dockerfile Linter

## Generating the lexer and parser for Linterd:

```
fslex --unicode DLex.fsl ;\
fsyacc --module DPar DPar.fsy ;\
fsharpi -r /etc/fsharp/FsLexYacc.Runtime.dll Absyn.fs Utils.fs DPar.fs \
DLex.fs Parse.fs Interp.fs ParseAndRun.fs
```

## Running with runArg.fs (for less typing):

```
mono $fslex --unicode DLex.fsl ;\
mono $fsyacc --module DPar DPar.fsy ;\
fsharpi -r /etc/fsharp/FsLexYacc.Runtime.dll Absyn.fs Utils.fs DPar.fs \
DLex.fs Parse.fs Interp.fs ParseAndRun.fs --use:runArg.fs
```

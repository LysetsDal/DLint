# Linterd
Prototype for Dockerfile Linter


Generating the lexer and parser for linterd:

```
    fslex --unicode DLexer.fsl
    fsyacc --module DParser DParser.fsy

    fsharpi -r /etc/fsharp/FsLexYacc.Runtime.dll Absyn.fs DParser.fs \
    DLexer.fs Parse.fs Interp.fs ParseAndRun.fs
```
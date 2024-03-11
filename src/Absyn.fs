
module Absyn

type instr =
    | From of string * string    (* Build Image <name>:<tag>       *)
    | Workdir of path            (* Specify working directory      *)
    | Copy of path * path        (* Copy from:<path> to:<path>     *)
    | Var of string              (* No Use so far                  *)
    | Expose of int  
    | Path of path               (* Expose a port of int           *)

and path =
    | DirPath of string

and dockerfile =
    | DFile of instr list          


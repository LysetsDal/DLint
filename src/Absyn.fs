
module Absyn

type stmt =
    | From of string * string    (* Build Image <name>:<tag>       *)
    | Workdir of path            (* Specify working directory      *)
    | Copy of path * path        (* Copy from:<path> to:<path>     *)
    | Var of string              (* No Use so far                  *)
    | Expose of int              (* Expose a port of int           *)

and path =
    | Path of string             (* A path to a directory          *)

and dockerfile =
    | File of stmt list          


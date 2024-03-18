module Absyn

type instr =
    | BaseImage of string * tag (* Build Image <name>:<tag> *)
    | Workdir of path (* Specify working directory  *)
    | Copy of path * path (* Copy from:<path> to:<path> *)
    | Var of string (* No Use so far  *)
    | Expose of expose (* Expose a port of int *)
    | User of string option * int option

and path = Dirs of dir list

and dir = Dir of string

and expose =
    | Port of int
    | Ports of int * int

and tag =
    | Tag of string
    | TagV of int * string

and dockerfile = DFile of instr list

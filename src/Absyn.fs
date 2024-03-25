module Absyn

type instr =
    
    | BaseImage of string * tag (* Build Image <name>:<tag> *)
    | Workdir of wpath (* Specify working directory  *)
    | Copy of cpath   (* Copy from:<path> to:<path> *)
    | Var of string (* No Use so far  *)
    | Expose of expose (* Expose a port of int *)
    | User of string option * int option
    | Run of cmd
    | EntryCmd of cmd
    | Env of env
    | Add of apath

and wpath = 
    | WPath of string

and cpath = 
    | CPath of string * string

and apath =
    | APath of string * string

and cmd = 
    | Cmd of string
    | Cmds of string list

and dir = Dir of string

and env = EnvVar of string * string

and expose =
    | Port of int
    | PortM of int * int
    | Ports of int list

and tag =
    | Tag of string
    | TagV of int * string

and dockerfile = DFile of instr list

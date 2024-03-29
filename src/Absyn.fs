// ================================================
//                ABSTRACT SYNTAX  
// ================================================

module Absyn

// Supported Dockerfile instructions
type instr =
    | BaseImage of string * tag   (* Build Image <name>:<tag>     *)
    | Workdir of wpath            (* Working directory <path>     *)
    | Copy of cpath               (* Copy from:<path> to:<path>   *)
    | Var of string               (* No Use so far                *)
    | Volume of mnt_pt            (* Volume mounts <mount_point>  *)
    | Expose of expose            (* Expose a port of int         *)
    | User of string option * int option (* Name, GUID or both    *)
    | Run of cmd                  (* Run parses shell cmds        *)
    | EntryCmd of cmd             (* Entry shell cmd of container *)
    | Env of env                  (* Key=value pairs of string    *)
    | Add of apath                (* Add files from path, to path *)

// A WORKDIR path (single)
and wpath = 
    | WPath of string

// A Volume mountpoint (single)
and mnt_pt = 
    | Mnt_pt of string
    
// A COPY path (double)
and cpath = 
    | CPath of string * string

// An ADD path (double)
and apath =
    | APath of string * string

// A Shell command (one or more)
and cmd = 
    | Cmd of string
    | Cmds of string list

// Environment Variables. A <Key>=<Value> pair of strings
and env = EnvVar of string * string

// Expose ports
and expose =
    | Port of int
    | PortM of int * int
    | Ports of int list

// Parse Docker image tags 
and tag =
    | Tag of string
    | TagV of int * string

// The Abstract representation of a dockerfile
and dockerfile = DFile of instr list

// ================================================
//                ABSTRACT SYNTAX  
// ================================================

module Absyn

// Supported Dockerfile instructions
type instr =
    | BaseImage of int * string * tag  (* Build Image <name>:<tag>     *)
    | Workdir of int * wpath           (* Working directory <path>     *)
    | Copy of int * cpath
    | Var of int * string              (* Type used to discard non-docker instructions *)
    | Volume of int * mnt_pt            (* Volume mounts <mount_point>  *)
    | Expose of int * expose  
    | User of int * (string option * int option) (* Name, GUID or both    *)
    | EntryCmd of int * cmd             (* Entry shell cmd of container *)
    | Run of int * cmd
    | Env of int * env
    | Add of int * apath                (* Add files from path, to path *)

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

// A Shell command 
and cmd =
    | Cmd of string 

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

// ================================================
//                ABSTRACT SYNTAX  
// ================================================

module Absyn

/// <summary> Supported Dockerfile instructions in the Abstract syntax </summary>
type instruction =
    | BaseImage of int * string * tag            (* Build Image <name>:<tag>     *)
    | Workdir of int * wpath                     (* Working directory <path>     *)
    | Copy of int * cpath
    | Var of int * string                        (* Discard invalid docker cmds  *)
    | Volume of int * mnt_pt                     (* Volume mounts <mount_point>  *)
    | Expose of int * expose  
    | User of int * (string option * int option) (* Name, GUID or both    *)
    | EntryCmd of int * shellcmd                 (* Entry shell cmd of container *)
    | Run of int * shellcmd
    | Env of int * env
    | Add of int * apath                         (* Add files from path, to path *)


/// <summary> A WORKDIR path (single) </summary>
and wpath = 
    | WPath of string


/// <summary> A Volume mountpoint (single) </summary>
and mnt_pt = 
    | Mnt_pt of string


/// <summary> A COPY path (double) </summary>
and cpath = 
    | CPath of string * string


/// <summary> An ADD path (double) </summary>
and apath =
    | APath of string * string


/// <summary> A Shell command </summary>
and shellcmd =
    | ShellCmd of string 


/// <summary> Environment Variables. A 'Key'='Value' pair of strings </summary>
and env = EnvVar of string * string


/// <summary> Expose ports </summary>
and expose =
    | Port of int
    | PortM of int * int
    | Ports of int list

/// <summary> Parse Docker image tags </summary>
and tag =
    | Tag of string
    | TagV of int * string

/// <summary> The Abstract representation of a dockerfile </summary>
and dockerfile = DFile of instruction list

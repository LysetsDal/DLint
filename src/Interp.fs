(*
    F# interpreter of the abstract syntax of a Dockerfile.
*)

module Interp

open Absyn

// type stmt =
//     | From of string * string        
//     | Workdir of path
//     | Copy of path * path
//     | Var of string
//     | Expose of int
//     | Path of path

// and path =
//     | DirPath of string
//     | MntPath of string

// and dockerfile =
//     | DFile of stmt list


(* Environment operations *)

type 'v env = (string * 'v) list

let rec lookup env x =
    match env with 
    | []        -> failwith (x + " not found")
    | (y, v)::r -> if x=y then v else lookup r x

type value = 
    | String of string
    | Int of int


let rec eval (s: instr) (env: value env) =
    match s with
    | BaseImage(img, tag) -> failwith "not implemented"
    | Workdir p -> failwith "not implemented"
    | Expose i -> failwith "not implemented"
    | _ -> failwith "Case not covered by eval"



let run s = eval s []

// let dfile = DFile([From("ubuntu", "latest"); Workdir(DirPath("/tmp/esc")); Expose(8080)])
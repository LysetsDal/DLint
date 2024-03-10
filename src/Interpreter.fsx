(*
    F# interpreter of the abstract syntax of a Dockerfile.
*)

// module Interpreter

// open Absyn

type stmt =
    | From of string * string        
    | Workdir of path
    | Copy of path * path
    | Var of string
    | Expose of int

and path =
    | Path of string

and dockerfile =
    | File of stmt list


(* Environment operations *)

type 'v env = (string * 'v) list

let rec lookup env x =
    match env with 
    | []        -> failwith (x + " not found")
    | (y, v)::r -> if x=y then v else lookup r x

type value = 
    | String of string


let rec eval (s: stmt) (env: value env) =
    match s with
    | From(img, tag) -> 





let run e = eval e []


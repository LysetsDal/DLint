// =======================================================
// F# interpreter of the abstract syntax of a Dockerfile.
// =======================================================

module Interp

open Absyn

let unpackDFile (dfile: dockerfile) : instr list =
    match dfile with
    | DFile instruction -> instruction


// Environment (store) functions 
type store = (int * instr) list

let emptyStore : (int * instr) list = List.empty 


// This function initializes the store that is used 
// to keep the docker file in memory.
let initStore (dfile: dockerfile) : store =
    let rec addInstr instructions counter store =
        match instructions with 
        | [] -> store
        | x :: rest -> 
            let newStore = (counter, x) :: store 
            addInstr rest (counter + 1) newStore
    addInstr (unpackDFile dfile) 0 []
    |> List.rev


// Print the content of the store (debug function)
let printStore (s: store) =
    printfn "\nSTORE CONTAINS: "
    let rec aux s =
        match s with
        | [] -> printfn ""
        | (idx, instr) :: rest -> 
            printfn $"%d{idx} = [ %A{instr} ]; "
            aux rest
    aux s
    

// Debug function
let returnStore (s: store) : instr list =
    let rec aux s acc =
        match s with
        | [] -> acc
        | (idx, instr) :: rest -> aux rest (instr :: acc)
    aux s []


//  Eval: this function should apply the rules all
//  the different types in the docker file.
let rec eval (s: instr) (store:  store) =
    match s with
    | BaseImage(name, tag) -> printfn "BaseImg {img: %s:%A}" name tag
    | Workdir path -> failwith "not implemented"
    | Copy path -> failwith "not  implemented"
    | Var v -> failwith "not implemented"
    | Expose x -> failwith "not implemented"
    | User(name, uid) -> failwith "not implemented"
    | Run cmd -> failwith "not implemented"
    | EntryCmd cmd -> failwith "not implemented"
    | Env e -> failwith "not implemented"
    | Add path -> failwith "not implemented"



// Run: The entry point function of the interpreter
let run dfile =
    let gstore = initStore dfile
    printStore gstore
    returnStore gstore


let df = DFile [BaseImage ("ubuntu", Tag "latest"); Run (Cmd "apt-get install "); Expose (PortM (80, 8080))]

let runTest =
    let gstore = initStore df
    printStore gstore
    returnStore gstore


// =======================================================
//        INTERPRETER FOR LINTING DOCKERFILES
// =======================================================
module Linterd.Engine.Interp

open Types
open Absyn


// The store is an in-memory representation of the dockerfile.
// It is used to perform the linters checks on.

let emptyStore : (int * instruction) list = List.empty 

// Unpacks a dfile to an instruction list.
let unpackDFile (dfile: dockerfile) : instruction list =
    match dfile with
    | DFile instruction -> instruction


// Initilize the store
let initStore (dfile: dockerfile) : store =
    let rec addInstr instructions counter store =
        match instructions with 
        | [] -> store
        | Var _ :: rest ->
            addInstr rest counter store  // <- Discarding the var-type
        | x :: rest -> 
            let newStore = (counter, x) :: store 
            addInstr rest (counter + 1) newStore
    addInstr (unpackDFile dfile) 0 emptyStore
    |> List.rev


// Print the content in the store
let printStore (s: store) =
    let rec aux s =
        match s with
        | [] -> printfn ""
        | (idx, instr) :: rest -> 
            printfn $"%d{idx} = [ %A{instr} ]; "
            aux rest
    aux s


// Return the instructions in the store
let returnStore (s: store) : instruction list =
    let rec aux s acc =
        match s with
        | [] -> List.rev acc
        | (_, instr) :: rest -> aux rest (instr :: acc)
    aux s []


// Take an instruction and return a list
let instrToRunCmd (ins: instruction) =
    match ins with
    | Run (line, ShellCmd cmd) -> Some (RunCommand.createCmd line cmd (RunCommand.split cmd))
    | _ -> None
        

// Extract RUN commands from instruction list
let getRunCmds (lst: instruction list)  =
    lst
    |> List.map instrToRunCmd
    |> List.choose id
    |> RunCommandList.createRunCommandList
    
    
// =======================================================
//                   Exposed Functions
// =======================================================

// Run: The 'main' logic of the interpreter
let run dfile =
    let gstore = initStore <| dfile  // Load dfile into store
    if Config.DEBUG then
        Logger.printHeaderMsg "INTERP @ initStore: STORE"
        printStore gstore
    
    // Transform dfile to instructions
    let instrs = returnStore <| gstore
    
    // Extract run cmds from instructions
    let rcmds = getRunCmds <| instrs   
    if Config.DEBUG then
        Logger.log Config.LOG_AS_CSV (LogHeader "INTERP @ getRunCmds: INSTRUCTIONS") 
        printfn $"%A{instrs}\n"
    
    // 1. Execute shellcheck
    if Config.DEBUG then
        Logger.log Config.LOG_AS_CSV (LogHeader "INTERP @ Shellcheck.scan: RCMDS")
        printfn $"%A{RunCommandList.runCommandListToString rcmds}\n"
     
    Shellcheck.scan <| rcmds
    if not Config.DEBUG then Shellcheck.flushTmpFiles  // Delete the tmp files 
    
    // 2. Scan other commands and binaries
    Binaries.scan <| rcmds
    
    // 3. Execute mount check
    Mounts.scan rcmds instrs

    // 4. Execute syntax check (Users)
    Syntax.scan instrs
    
    // 5. Scan network interface + ports
    Network.scan rcmds instrs 

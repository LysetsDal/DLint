// =======================================================
//        INTERPRETER FOR LINTING DOCKERFILES
// =======================================================
module Linterd.Interpreter

open Linterd.Engine
open Absyn
open Types


/// <summary>
/// The store is an in-memory representation of the dockerfile.
/// It is used to perform the linters checks on
/// </summary>
let emptyStore : (int * instruction) list = List.empty 


/// <summary> Unpacks a dfile to an instruction list </summary>
/// <param name="dfile"> A dockerfile (instruction list) </param>
let unpackDFile (dfile: dockerfile) : instruction list =
    match dfile with
    | DFile instruction -> instruction


/// <summary> Initilize the store with the dockerfile as Abstract Syntax </summary>
/// <param name="dfile"> An dockerfile (instruction list) </param>
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



/// <summary> Return the instructions in the store </summary>
/// <param name="s"> The active store </param>
let returnStore (s: store) : instruction list =
    let rec aux s acc =
        match s with
        | [] -> List.rev acc
        | (_, instr) :: rest -> aux rest (instr :: acc)
    aux s []


/// <summary> Take an instruction and return a RunCommand  </summary>
/// <param name="ins"> An instruction to transform </param>
let instrToRunCmd (ins: instruction) =
    match ins with
    | Run (line, ShellCmd cmd) -> Some (RunCommand.createCmd line cmd (RunCommand.split cmd))
    | _ -> None
        


/// <summary> Extract RUN commands from instruction list </summary>
/// <param name="lst"> An instruction list to get a RunCommandList from </param>
let getRunCmds (lst: instruction list)  =
    lst
    |> List.map instrToRunCmd
    |> List.choose id
    |> RunCommandList.createRunCommandList
    
    

/// <summary> Controls the logic of the interpreter </summary>
/// <param name="dfile"> A dockerfile (instruction list), to run the interpreter on </param>
let run dfile =
    let gstore = initStore <| dfile  // Load dfile into store
    if Config.DEBUG then
        Logger.printHeaderMsg "INTERP @ initStore: STORE"
        Logger.printStore gstore
    
    // Transform dfile to instructions
    let instrs = returnStore <| gstore
    
    // Extract run cmds from instructions
    let rcmds = getRunCmds <| instrs   
    if Config.DEBUG then
        Logger.log (LogHeader "INTERP @ getRunCmds: INSTRUCTIONS") 
        printfn $"%A{instrs}\n"
    
    // 1. Execute shellcheck
    if Config.DEBUG then
        Logger.log (LogHeader "INTERP @ Shellcheck.scan: RCMDS")
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

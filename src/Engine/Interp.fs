// =======================================================
//        INTERPRETER FOR LINTING DOCKERFILES
// =======================================================

module Linterd.Engine.Interp

open Absyn             


module private StoreInternals =
    // The store is an in-memory representation of the dockerfile.
    // It is used to perform the linters checks on.
    type store = (int * instr) list

    let emptyStore : (int * instr) list = List.empty 
    
    // Unpacks a dfile to an instruction list.
    let unpackDFile (dfile: dockerfile) : instr list =
        match dfile with
        | DFile instruction -> instruction
    
    // let purgeVars (lst: instr list) =
    //     let rec aux lst =
    //         match lst with
    //         | Var x -> 

    // Initilize the store
    let initStore (dfile: dockerfile) : store =
        let rec addInstr instructions counter store =
            match instructions with 
            | [] -> store
            | Var _ :: rest ->
                addInstr rest counter store  // <- Discarding the vars type
            | x :: rest -> 
                let newStore = (counter, x) :: store 
                addInstr rest (counter + 1) newStore
        addInstr (unpackDFile dfile) 0 emptyStore
        |> List.rev


    // Print the content in the store
    let printStore (s: store) =
        printfn "\nNR. -  STORE CONTAINS: "
        let rec aux s =
            match s with
            | [] -> printfn ""
            | (idx, instr) :: rest -> 
                printfn $"%d{idx} = [ %A{instr} ]; "
                aux rest
        aux s
    
    
    // Return the instructions in the store
    let returnStore (s: store) : instr list =
        let rec aux s acc =
            match s with
            | [] -> List.rev acc
            | (_, instr) :: rest -> aux rest (instr :: acc)
        aux s []


module private RunCommands =
    // A predicate filter for Run instructions
    let isRunInstr (ins: instr) =
        match ins with
        | Run _ -> true
        | _ -> false
        
        
    // Take an instruction and return a list
    let instrToCmdList (ins: instr) =
        match ins with
        | Run (idx, Cmd cmd) -> [cmd]
        | _ -> []
     

    // Extract RUN commands from instruction list
    let getRunCmds (lst: instr list) : string list =
        lst
        |> List.filter isRunInstr                             // 1. Predicate
        |> List.collect instrToCmdList                       // 2. Unwrap instruction
        |> List.fold (fun acc x -> (acc @ Utils.split x)) []         // 3. Split runcommand


// =======================================================
//                   Exposed Functions
// =======================================================
open StoreInternals
open RunCommands

// Run: The 'main' logic of the interpreter
let run dfile =
    let gstore = initStore <| dfile  // Load dfile into store
    if Config.DEBUG then printStore gstore
    
    // Transform dfile to instructions
    let instrs = returnStore <| gstore
    printfn "INSTRUCTIONS:"
    printfn $"%A{instrs}\n"
    
    // Extract run cmds from instructions
    let rcmds = getRunCmds <| instrs  
    
    // 1. Execute shellcheck
    if Config.VERBOSE then Utils.printStringList rcmds "RUNCMDS LIST" 
    Shellcheck.scan <| rcmds
    // Shellcheck.flush Config.OUTPUT_DIR
    

    // 2. Scan other commands and binaries
    Binaries.scan <| rcmds
    
    
    // 3. Scan network interface + ports
    let netcmds = Network.runNetworkCheck <| rcmds
    if Config.VERBOSE then Utils.printStringList netcmds "NETCMDS LIST"
    Network.scan <| netcmds <| instrs 
    

    
    
    // 4. Execute mount check
    let vmnts = Mounts.getVolumeMounts <| instrs
    let rmnts = Mounts.getRunMounts <| rcmds
    
    if Config.VERBOSE then Utils.printStringList vmnts "VOLMOUNTS LIST"
    if Config.VERBOSE then Utils.printStringList rmnts "RUNMOUNTS LIST"
    
    Mounts.scan vmnts
    Mounts.scan rmnts

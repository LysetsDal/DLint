// =======================================================
// F# interpreter of the abstract syntax of a Dockerfile.
// =======================================================

module Interp

open System.Diagnostics
open System.IO
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
    
let printStringList (lst: string list) =
    printfn "\nLIST: "
    let rec aux lst = 
        match lst with
        | [] -> printfn ""
        | x :: rest ->
            printfn $"[ %s{x} ]"
            aux rest
    aux lst

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


// ===============================================
//             Shell check handling
// ===============================================

// Path to shellcheck binary
let shellcheck = "../shellcheck/shellcheck"    

// Predicate filter
let isShellCmd (ins: instr) =
    match ins with
    | Run _ -> true
    | _ -> false
    
// Take an instruction and return a lsit
let shellCmdToLst (ins: instr) =
    match ins with
    | Run (Cmd cmd) -> [cmd]
    | Run (Cmds cmds) -> cmds
    | _ -> failwith "Unexpected type"
    
    
// split at delimiter(s) ;
let splitCmdAt (delim: string[]) (cmd: string) =
    List.ofArray (cmd.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))

// standard shell delimiters
let standardSplitCmd (cmd: string)  =
    let delims = [|"&&"; ";"; "|"; "<<"; ">>"|]
    splitCmdAt delims cmd
    
    
// Append shebang to each string
let appendSheBang s = "#!/bin/bash" + "\n" + s

// Appply appendShebang to list of strings
let appendSheBangs (lst: string list) =
    let rec aux lst acc =
        match lst with  
        | [] -> acc 
        | x :: rest -> aux rest (appendSheBang x :: acc)
    aux (List.rev lst) []



// Extract RUN commands from instruction list
let getShellCmds (lst: instr list) : string list =
    lst
    |> List.filter isShellCmd                                    // 1. predicate
    |> List.collect shellCmdToLst                               // 2. unwrap instruction
    |> List.fold (fun acc x -> (acc @ standardSplitCmd x)) []   // 3. split runcommand
    |> appendSheBangs                                                   // 4. prepare for tmp-file
    

// Create a temporary shell file (used to to invoke shellcheck on)
let createShellFile (filepath: string) (cmd: string) =
    let file = File.WriteAllText(filepath, cmd)
    File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
    

let closeShellFile (stream: FileStream) =
    stream.Close()
    
    
let deleteTmpFile (filePath: string) =
    try
        File.Delete(filePath)
        printfn $"File '%s{filePath}' deleted successfully."
    with
    | :? System.IO.DirectoryNotFoundException ->
        printfn $"File '%s{filePath}' not found."
    | :? System.IO.IOException ->
        printfn $"An error occurred while deleting file '%s{filePath}'."


let flushTmpFiles (directoryPath: string) =
    let files = Directory.GetFiles(directoryPath)
    for file in files do
        deleteTmpFile file


// Spawn a new shellcheck process that returns the output
let executeShellCheck (shellcheck: string) (file: string) (input: FileStream) =
    let processInfo = new ProcessStartInfo(shellcheck, $"-s bash -f gcc %s{file}")
    processInfo.RedirectStandardInput <- true
    processInfo.RedirectStandardOutput <- true
    processInfo.UseShellExecute <- false
    
    let thread = Process.Start(processInfo)
    use writer = thread.StandardInput
    use reader = thread.StandardOutput
    writer.WriteLine(input)
    writer.Close()
    
    processInfo.RedirectStandardInput <- false
    processInfo.RedirectStandardOutput <- false
    let output = reader.ReadToEnd()
    thread.WaitForExit()
    thread.Close()
    output

    
// Run: The entry point function of the interpreter
let run dfile =
    let gstore = initStore dfile
    printStore gstore 
    let instrs = returnStore gstore
    
    let cmds = getShellCmds instrs
    printStringList cmds  
    

    let mutable count = 0;
    for cmd in cmds do
        let fpath = $"./out/cmd_%s{string count}"
        let tmp_file = createShellFile fpath cmd
        let res = executeShellCheck shellcheck fpath tmp_file
        closeShellFile tmp_file
        count <- count + 1
        printfn $"Output : {res}"
    done
        
    // Delete tmp files
    // flushTmpFiles "./out/"


// Test Purposes:
let df = DFile [BaseImage ("ubuntu", Tag "latest"); Run (Cmd "apt-get install ");
                Expose (PortM (80, 8080))]






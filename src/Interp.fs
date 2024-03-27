// =======================================================
//        INTERPRETER FOR LINTING DOCKERFILES
// =======================================================

module Interp

open System.Diagnostics  // Threads & Processes
open System.IO          // File manipulation 
open Absyn             // Abstract Syntax


// Unpacks a dfile to an instruction list
let unpackDFile (dfile: dockerfile) : instr list =
    match dfile with
    | DFile instruction -> instruction


// The store is an in-memory abstraction representation of the
// docker file. It is used to perform the linters checks on
type store = (int * instr) list
let emptyStore : (int * instr) list = List.empty 


// Initilize the store
let initStore (dfile: dockerfile) : store =
    let rec addInstr instructions counter store =
        match instructions with 
        | [] -> store
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
        | [] -> acc
        | (idx, instr) :: rest -> aux rest (instr :: acc)
    aux s []


// ===============================================
//             Shell check handling
// ===============================================

// A predicate filter
let isShellCmd (ins: instr) =
    match ins with
    | Run _ -> true
    | _ -> false
    
    
// Take an instruction and return a list
let shellCmdToLst (ins: instr) =
    match ins with
    | Run (Cmd cmd) -> [cmd]
    | Run (Cmds cmds) -> cmds
    | _ -> failwith "Unexpected type"
    
    
// Split at delimiter(s)
let splitCmdAt (delim: string[]) (cmd: string) =
    List.ofArray (cmd.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))


// Split with stnadard shell delimiters (See Config.fs)
let standardSplitCmd (cmd: string)  =
    let delims = Config.SHELL_CMD_DELIMS
    splitCmdAt delims cmd
    
    
// Append shebang to each string
let appendSheBang s = Config.SHEBANG + s


// Appply appendShebang to list of strings
let appendSheBangs (lst: string list) =
    let rec aux lst acc =
        match lst with  
        | [] -> acc 
        | x :: rest -> aux rest (appendSheBang x :: acc)
    aux (List.rev lst) []



// Extract RUN commands from instruction list
let getRunCmds (lst: instr list) : string list =
    lst
    |> List.filter isShellCmd                                    // 1. Predicate
    |> List.collect shellCmdToLst                               // 2. Unwrap instruction
    |> List.fold (fun acc x -> (acc @ standardSplitCmd x)) []   // 3. Split runcommand
    |> appendSheBangs                                                   // 4. Prepare for tmp-file
    

// Create a temporary shell file (used to to invoke shellcheck on)
let createShellFile (filepath: string) (cmd: string) =
    File.WriteAllText(filepath, cmd)
    File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
    

// Close an open file(stream) 
let closeShellFile (stream: FileStream) =
    stream.Close()
    

// Delete the tmpfiles created for the shellcheck integration
let deleteTmpFile (filePath: string) verbose =
    try
        File.Delete(filePath)
        if  verbose then
            printfn $"File '%s{filePath}' deleted successfully."
    with
    | :? DirectoryNotFoundException ->
        printfn $"File '%s{filePath}' not found."
    | :? IOException ->
        printfn $"An error occurred while deleting file '%s{filePath}'."


// Delete all  
let deleteAllFiles (directoryPath: string) =
    let files = Directory.GetFiles(directoryPath)
    for file in files do
        deleteTmpFile file false
    printfn $"Files at '%s{directoryPath}' deleted successfully."


// Spawn a shellcheck process from a context.
// Returns the output of shellcheck applied to that context
let shellCheckFile (shellcheck: string) (file: string) (input: FileStream) =
    // Define a new context for a shellcheck process
    let shellcheckStartCmd = $"%s{Config.SHELLCHECK_ARGS} %s{file}"
    let processInfo = ProcessStartInfo(shellcheck, shellcheckStartCmd)
    processInfo.RedirectStandardInput <- true
    processInfo.RedirectStandardOutput <- true
    processInfo.UseShellExecute <- false
    
    // Spawn the new thread with the process context
    let thread = Process.Start(processInfo)
    use writer = thread.StandardInput
    use reader = thread.StandardOutput
    writer.WriteLine(input)
    writer.Close()
    
    // Reirect output back to main-process again
    processInfo.RedirectStandardInput <- false
    processInfo.RedirectStandardOutput <- false
    let output = reader.ReadToEnd()
    thread.WaitForExit()
    thread.Close()
    output

// Perform the shcellChek on the given commands
let executeShellCheck cmds =
    let mutable count = 0
    for cmd in cmds do
        let fpath = $"%s{Config.OUTPUT_DIR}cmd_%s{string count}"
        let tmp_file = createShellFile fpath cmd                // Create the tmp. file
        
        let res = shellCheckFile Config.SHELLCHECK fpath tmp_file  // Spawn a new shellcheck process
        closeShellFile tmp_file
        count <- count + 1
        
        if res <> "" then
            printfn $"{res}"

    
// Run: The entry point of the interpreter
let run dfile =
    let gstore = initStore dfile  // Load dfile into store
    if Config.DEBUG then
        printStore gstore
    
    // Transform dfile to instructions
    let instrs = returnStore gstore
    let cmds = getRunCmds instrs 
    Utils.printStringList cmds  
    
    
    // Execute the shellcheck 
    executeShellCheck cmds
        
    // Delete tmp files
    deleteAllFiles Config.OUTPUT_DIR

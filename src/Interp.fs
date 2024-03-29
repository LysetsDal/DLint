// =======================================================
//        INTERPRETER FOR LINTING DOCKERFILES
// =======================================================

module Interp

open System.Diagnostics  // Threads & Processes
open System.IO           // File manipulation 
open Absyn               // Abstract Syntax


// Unpacks a dfile to an instruction list.
let unpackDFile (dfile: dockerfile) : instr list =
    match dfile with
    | DFile instruction -> instruction


// The store is an in-memory representation of the dockerfile.
// It is used to perform the linters checks on.
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
        | (_, instr) :: rest -> aux rest (instr :: acc)
    aux s []


// ===============================================
//             Shell check handling
// ===============================================

// A predicate filter for Run instructions
let isRunInstr (ins: instr) =
    match ins with
    | Run _ -> true
    | _ -> false
    

    
// Take an instruction and return a list
let instrToCmdList (ins: instr) =
    match ins with
    | Run (Cmd cmd) -> [cmd]
    | Run (Cmds cmds) -> cmds
    | _ -> []


// Is a Run '--mount' command
let  isRunMountCmd (lst: string list) =
    let mount_prefix = "--mount"
    
    let rec aux (lst: string list) acc =
        match lst with
        | [] -> acc
        | x :: rest ->
            match x.StartsWith mount_prefix with
            | true ->  aux rest (x :: acc)
            | _ ->  aux rest acc
    aux (List.rev lst) []
            
    
// Split at delimiter(s)
let splitCmdAt (delim: string[]) (cmd: string) =
    List.ofArray (cmd.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))


// Split with stnadard shell delimiters (See Config.fs)
let standardSplitCmd (cmd: string)  =
    let delims = Config.SHELL_CMD_DELIMS
    splitCmdAt delims cmd
    
    
// Append shebang to each string
let prependSheBang s = Config.SHEBANG + s


// Appply appendShebang to list of strings
let prependSheBangs (lst: string list) =
    let rec aux lst acc =
        match lst with  
        | [] -> acc 
        | x :: rest -> aux rest (prependSheBang x :: acc)
    aux (List.rev lst) []



// Extract RUN commands from instruction list
let getRunCmds (lst: instr list) : string list =
    lst
    |> List.filter isRunInstr                                     // 1. Predicate
    |> List.collect instrToCmdList                               // 2. Unwrap instruction
    |> List.fold (fun acc x -> (acc @ standardSplitCmd x)) []    // 3. Split runcommand
    |> prependSheBangs                                                   // 4. Prepare for tmp-file
    

// Create a temporary shell file (used to to invoke shellcheck on)
let openOrCreateRWFile (filepath: string) (cmd: string) =
    File.WriteAllText(filepath, cmd)
    File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
    

// Close an open file(stream) 
let closeFile (stream: FileStream) =
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
        let tmp_file = openOrCreateRWFile fpath cmd                   // Create the tmp. file
        
        let res = shellCheckFile Config.SHELLCHECK fpath tmp_file  // Spawn a new shellcheck process
        closeFile tmp_file
        count <- count + 1
        
        if res <> "" then
            printfn $"{res}"
            
// ================================================
//                Linting for Mounts
// ================================================

// A predicate filter for Volume instructions
let isVolume (ins: instr) =
    match ins with
    | Volume _ -> true
    | _ -> false

// Unpack a Volume to an instruction
let volumeToList (ins: instr) =
    match ins with
    | Volume (Mnt_pt cmd) -> [cmd]
    
    | _ -> failwith "Unexpected type"

    
// Extract RUN commands from instruction list
let getVolumeMounts (lst: instr list) : string list =
    lst
    |> List.filter isVolume                     // 1. Predicate
    |> List.collect volumeToList                       // 2. Unwrap instruction
   
   
open Rules.MountWarn
open System.Text.RegularExpressions

let isNullOrWhiteSpace (str: string) =
    str = null || str.Trim() = ""

// 
let extractSensitiveMountsFromFile (filePath: string) =
    let fileContents = File.ReadAllText(filePath)
    let mountRegex = Regex(@"MountPoint\s*=\s*""([^""]+)""")
    let mountMatches = mountRegex.Matches(fileContents) |> Seq.map (_.Groups[1].Value)
    let codeRegex = Regex(@"Code\s*=\s*""([^""]+)""")
    let msgRegex = Regex(@"Msg\s*=\s*""([^""]+)""")
    

    let code = 
        match codeRegex.Match(fileContents).Groups[1].Value with
        | code when not (isNullOrWhiteSpace code) -> code
        | _ -> ""
    
    let msg =
        match msgRegex.Match(fileContents).Groups[1].Value with
        | msg when not (isNullOrWhiteSpace msg) -> msg
        | _ -> ""

    [ for mount in mountMatches -> { Code = code; MountPoint = mount; Msg = msg } ]


// Sequence of all SensitiveMount objects 
let compareStringWithMountPoints (directoryPath: string) =
    let sensitiveMounts =
        Directory.GetFiles(directoryPath)
        |> Seq.collect extractSensitiveMountsFromFile
    sensitiveMounts

    

// Get RUN mount commands from instruction list
let getRunMounts (lst: instr list) : string list =
    lst
    |> List.filter isRunInstr                                    // 1. Predicate
    |> List.collect instrToCmdList                              // 2. Unwrap instruction
    |> List.fold (fun acc x -> (acc @ standardSplitCmd x)) []   // 3. Split runcommand
    |> isRunMountCmd


// Loops through the provided mounts and looks for matches.
// with known sensitive mounts.
let executeMountScan mounts =
    let sensitiveMountsSeq = compareStringWithMountPoints Config.RULE_DIR
    
    for mnt in mounts do
        Seq.iter (fun x ->
            match x with
            | _ when x.MountPoint = mnt ->
                printfn $"%s{x.Code}:\nSensetive Mount:%s{x.MountPoint}\nInfo message: %s{x.Msg}\n"
                //@TODO
                // Do soemthing other than printing
            | _ -> printf ""
        ) sensitiveMountsSeq
        
        
        

   
// Run: The 'main' logic of the interpreter
let run dfile =
    let gstore = initStore dfile  // Load dfile into store
    if Config.DEBUG then printStore gstore
    
    // Transform dfile to instructions
    let instrs = returnStore gstore
    
    
    // 1. Execute the shellcheck
    let cmds = getRunCmds instrs 
    if Config.VERBOSE then Utils.printStringList cmds  
    executeShellCheck cmds
    deleteAllFiles Config.OUTPUT_DIR
    
    //@TODO
    
    
    
    // 2. Execute mount check
    let vmnts = getVolumeMounts instrs
    let rmnts = getRunMounts instrs
    
    //run mounts should per default give a warning to not use that
    Utils.printStringList vmnts
    Utils.printStringList rmnts
    
    executeMountScan vmnts
    executeMountScan rmnts
    
    
    
    
    
// ===============================================
//             SHELL CHECK INTEGRATION 
// ===============================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Shellcheck

open System.IO           // File manipulation 
open System.Diagnostics  // Threads & Processes
            

module private Helpers =
    // Append shebang to each string
    let prependSheBang s = Config.SHEBANG + s
    
    // Appply appendShebang to list of strings
    let prependSheBangs (lst: string list) =
        let rec aux lst acc =
            match lst with  
            | [] -> acc 
            | x :: rest -> aux rest (prependSheBang x :: acc)
        aux (List.rev lst) []
        
        
module private InputOutput =
    // Create a temporary shell file (used to to invoke shellcheck on)
    let openOrCreateRWFile (filepath: string) (cmd: string) =
        File.WriteAllText(filepath, cmd)
        File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
        

    let closeFile (stream: FileStream) =
        stream.Close()
        
        
    // Delete the tmpfiles created for the shellcheck scan
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


module private ShellChekInternals =
    // Spawn a shellcheck process and redirect stdin + stdout.
    // Returns the output of shellcheck applied to that context
    let shellcheck (shellcheck: string) (file: string) (input: FileStream) =
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
        

// =======================================================
//                   Exposed Functions
// =======================================================

open ShellChekInternals
open InputOutput
open Helpers


// Delete all tmp files
let flush (directoryPath: string) =
    let files = Directory.GetFiles(directoryPath)
    for file in files do
        deleteTmpFile file false
    printfn $"Files at '%s{directoryPath}' deleted successfully."


// Perform the shcellChek on the given commands
let scan (cmds: string list) =
    let mutable count = 0
    
    // the --mount is a docker specific cmd. Hence shellcheck discards it.
    let filter_cmds =
        cmds
        |> List.except (Utils.getCmdByPrefix cmds "--mount")
        |> prependSheBangs
    
    for cmd in filter_cmds do
        let fpath = $"%s{Config.OUTPUT_DIR}cmd_%s{string count}"
        let tmp_file = openOrCreateRWFile fpath cmd               // Create the tmp. file
        
        let res = shellcheck Config.SHELLCHECK fpath tmp_file    // Spawn a new shellcheck process
        closeFile tmp_file
        count <- count + 1
        
        if res <> "" then
            printfn $"{res}"
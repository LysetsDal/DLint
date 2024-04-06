// ===============================================
//             SHELL CHECK INTEGRATION 
// ===============================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Shellcheck

open System.IO           // File manipulation 
open System.Diagnostics  // Threads & Processes
open Infrastructure

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
    
    
    // Prepend shebangs to Cmd.
    let prependShebangToCmd(cmd: Cmd) =
        let _, lst = cmd.LineNum, Cmd.getList cmd
        let shebang_list = prependSheBangs lst
        Cmd.createCmdWithLst cmd.LineNum cmd.Raw (Some shebang_list)
        
        
        
    // Filters each instructions associated list of splitted commands
    let filterInstructionByPrefix (cmd: Cmd) (prefix: string) =
        let filterList = Cmd.filterList prefix cmd
        let transformedCmd = (Utils.reconstructToString filterList).Trim('[', ']')
        Cmd.createCmd cmd.LineNum transformedCmd (Cmd.split transformedCmd)

                
    // Filter a list of instructions by the specified prefix 
    let filterInstructionsByPrefix (cmds: Cmds) (prefix: string) =
        let cmd_lst = cmds.List
        // printfn $"\n FILTER INSTR BEFORE :%A{cmd_lst}\n"
        let res = List.map (fun cmd -> filterInstructionByPrefix cmd prefix) cmd_lst
        // printfn $"\n FILTER INSTR AFTER %A{res}\n"
        res
        
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
    open InputOutput
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
        
        
    //@TODO: SPLIT SHELLCHECK OUTPUT
    // current output: ./tmp/cmd_0:2:29: note: Double quote to prevent globbing and word splitting. [SC2086]
    // desired: Double quote to prevent globbing and word splitting. [SC2086] (lst: int * string list)
    let runShellCheck (cmds : Cmds) =
        let mutable count = 0
        
        cmds.List
        |> List.iter ( fun cmd ->
            let idx = cmd.LineNum
            
            let rawCmd = (Cmd.getRaw cmd)
            let exploded_cmd = List.rev <| Utils.splitCmdAt [|";"|] rawCmd 
            
            
            exploded_cmd
            |> List.iter ( fun unit ->
                let fpath = $"%s{Config.OUTPUT_DIR}cmd_%s{string count}"
                let tmp_file = openOrCreateRWFile fpath unit    // Create the tmp. file
            
                let res = shellcheck Config.SHELLCHECK fpath tmp_file    // Spawn a new shellcheck process
                closeFile tmp_file
                
                if res <> "" then
                    let line = idx + (count - 1)
                    printfn $"Around Line: %i{line}\n{res}"
                count <- count + 1
            )
        )
        

// =======================================================
//                   Exposed Functions
// =======================================================
open ShellChekInternals
open InputOutput
open Helpers

// Delete all tmp files
let flushTmpFiles  =
    let files = Directory.GetFiles(Config.OUTPUT_DIR)
    for file in files do
        deleteTmpFile file false
    printfn $"Files at '%s{Config.OUTPUT_DIR}' deleted successfully."


// Perform the shcellChek on the given commands
let scan (cmds: Cmds) =
    // the RUN --mount is a docker specific cmd. Hence we discardd it before shellcheck.
    let filtered_cmds = filterInstructionsByPrefix cmds "--mount"
    printfn $"(SCAN) FILTERED CMDS: %A{filtered_cmds}\n"
    
    let shebang_cmds =
        filtered_cmds
        |> List.map prependShebangToCmd
        |> Cmds.createCmds 
    
    runShellCheck shebang_cmds
         
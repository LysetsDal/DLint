// ===============================================
//             SHELL CHECK INTEGRATION 
// ===============================================
[<RequireQualifiedAccess>]
module DLint.Engine.Shellcheck

open System.Diagnostics
open System.IO      
open Types

module private Helpers =
    
    /// <summary> Append '#!/bin/bash \n' (Shebang) to string </summary>
    /// <param name="s"> String to prepend </param>
    let prependSheBang s = Config.SHEBANG_PREFIX + s
    
    
    /// <summary> Appply appendShebang to list of strings </summary>
    /// <param name="lst"> list of strings to prepend </param>
    let prependSheBangs (lst: string list) =
        let rec aux lst acc =
            match lst with  
            | [] -> acc 
            | x :: rest -> aux rest (prependSheBang x :: acc)
        aux (List.rev lst) []
    
    
    /// <summary> Prepend shebangs to Cmd's internal list </summary>
    /// <param name="cmd"> Cmd whose internal list to prepend to </param>
    let prependShebangToCmd(cmd: RunCommand) =
        let line, lst = cmd.LineNum, RunCommand.getAsList cmd
        let shebang_list = prependSheBangs lst
        RunCommand.createCmdWithList line cmd.AsString (Some shebang_list)
    
    
    /// <summary> Used for striping the shabang off after shellcheck has run </summary>
    /// <param name="str"> string to strip shebang from </param>
    let stripSheBang (str:string) =
        match str with
        | _ when str.StartsWith("#!/bin/bash") || str.StartsWith("#!/usr/bin/bash") -> 
        str.Substring(str.IndexOf('\n') + 1)
        | _ -> str
        
        
module private InputOutput =
    /// <summary> Create a temporary file with shellcode in (used to to invoke shellcheck on) </summary>
    /// <param name="filepath"> File creation path (relative to fsproj file) </param>
    /// <param name="cmd"> cmd (as string) to write to file </param>
    let openOrCreateRWFile (filepath: string) (cmd: string) =
        File.WriteAllText(filepath, cmd)
        File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
    
    
    /// <summary> Close a filestream </summary>
    /// <param name="stream"> filestream to close </param>
    let closeFile (stream: FileStream) =
        stream.Close()
    
    
    /// <summary> Delete the tmpfiles created for the shellcheck scan </summary>
    /// <param name="file_path"> File to delete </param>
    /// <param name="verbose"> Set verbose (bool) printing mode </param>
    let deleteTmpFile (file_path: string) verbose =
        try
            File.Delete(file_path)
            if  verbose then
                printfn $"File '%s{file_path}' deleted successfully."
        with
        | :? DirectoryNotFoundException ->
            printfn $"File '%s{file_path}' not found."
        | :? IOException ->
            printfn $"An error occurred while deleting file '%s{file_path}'."


module private ShellChekInternals =
    open InputOutput
    open Helpers 

    /// <summary>Spawn a shellcheck process and redirect stdin + stdout. Returns the output of shellcheck applied to that context. </summary>
    /// <param name="shellcheck">The path to chellcheck</param>
    /// <param name="file">The tmp filename</param>
    /// <param name="input">A filestream</param>
    let shellcheck (shellcheck: string) (file: string) (input: FileStream) =
        // Define a new context for a shellcheck process
        let shellcheck_start_cmd = $"%s{Config.SHELLCHECK_ARGS} %s{file}"
        let process_info = ProcessStartInfo(shellcheck, shellcheck_start_cmd)
        process_info.RedirectStandardInput <- true
        process_info.RedirectStandardOutput <- true
        process_info.UseShellExecute <- false
        
        // Spawn the new thread with the process context
        let thread = Process.Start(process_info)
        use writer = thread.StandardInput
        use reader = thread.StandardOutput
        writer.WriteLine(input)
        writer.Close()
        
        // Reirect output back to main-process again
        process_info.RedirectStandardInput <- false
        process_info.RedirectStandardOutput <- false
        let output = reader.ReadToEnd()
        thread.WaitForExit()
        thread.Close()
        output
        
        
    /// <summary> Controls and runs the main shellcheck logic </summary>
    /// <param name="cmds"> A list of runcommands to run shellcheck on </param>
    let runShellCheck (cmds : RunCommandList) =
                
        cmds.List
        |> List.iter ( fun cmd ->
            let mutable count = 0   
            let idx = cmd.LineNum
            
            let cmd_list = RunCommand.getAsList cmd
            if Config.DEBUG then printfn $"SHELLCHECK @ runShellCheck: \n%A{cmd_list}\n"
                
            cmd_list
            |> List.iter ( fun unit ->
                // Create the tmp. file
                let file_path = $"%s{Config.OUTPUT_DIR}cmd_%s{string count}"
                let tmp_file = openOrCreateRWFile file_path unit    
            
                // Spawn a new shellcheck process and split output
                let mutable shellcheck_output = ""
                try
                    shellcheck_output <- shellcheck Config.SHELLCHECK file_path tmp_file    
                    closeFile tmp_file
                with 
                    | :? IOException as err -> printfn $"%s{err.Message}"
                
                
                // If shellcheck found something
                if shellcheck_output <> "" then
                    let split_output = shellcheck_output.Split(":")
                    let line = idx + count
                    let problem = stripSheBang unit
                    let char_pos = split_output[2]
                    let response_msg = split_output[4].Trim()
                    
                    Logger.log <| LogShellcheckWarn(line, char_pos, problem, response_msg)

                count <- count + 1
            )
            count <- 0
        )
        

// =======================================================
//                   Exposed Functions
// =======================================================
open ShellChekInternals
open InputOutput
open Helpers


/// <summary> Delete all tmp files </summary>
let flushTmpFiles  =
    let files = Directory.GetFiles(Config.OUTPUT_DIR)
    for file in files do
        deleteTmpFile file false
    if Config.VERBOSE then Logger.log <| FlushFiles


/// <summary> Perform the shcellChek on the given commands </summary>
/// <param name="cmds"> List of runcommands to check</param>
let scan (cmds: RunCommandList) =
    
    let filtered_cmds =
        cmds
        |> RunCommandList.exlcudePrefixedCmds "--mount"
        |> RunCommandList.createRunCommandList
        |> RunCommandList.exlcudePrefixedCmds "--network"
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "SHELLCHECK @ scan: FILTERED CMDS")
        printfn $"%A{filtered_cmds}\n"
    
    filtered_cmds
    |> List.map prependShebangToCmd
    |> RunCommandList.createRunCommandList 
    |> runShellCheck 
         
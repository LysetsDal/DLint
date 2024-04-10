// ===============================================
//             SHELL CHECK INTEGRATION 
// ===============================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Shellcheck

open System.Diagnostics
open Infrastructure
open System.IO      

module private Helpers =
    
    // Append shebang to each string
    let prependSheBang s = Config.SHEBANG_PREFIX + s
    
    
    // Appply appendShebang to list of strings
    let prependSheBangs (lst: string list) =
        let rec aux lst acc =
            match lst with  
            | [] -> acc 
            | x :: rest -> aux rest (prependSheBang x :: acc)
        aux (List.rev lst) []
    
    
    // Prepend shebangs to Cmd.
    let prependShebangToCmd(cmd: RunCommand) =
        let line, lst = cmd.LineNum, RunCommand.getAsList cmd
        let shebang_list = prependSheBangs lst
        RunCommand.createCmdWithList line cmd.AsString (Some shebang_list)
    
    
    // Used for printing the problematic command after shellcheck
    let stripSheBang (str:string) =
        match str with
        | _ when str.StartsWith("#!/bin/bash") || str.StartsWith("#!/usr/bin/env") -> 
        str.Substring(str.IndexOf('\n') + 1)
        | _ -> str
        
        
module private InputOutput =
    // Create a temporary shell file (used to to invoke shellcheck on)
    let openOrCreateRWFile (filepath: string) (cmd: string) =
        File.WriteAllText(filepath, cmd)
        File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
    
    
    let closeFile (stream: FileStream) =
        stream.Close()
    
        
    // Delete the tmpfiles created for the shellcheck scan
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
    // Spawn a shellcheck process and redirect stdin + stdout.
    // Returns the output of shellcheck applied to that context
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
        
    // Controls the shellcheck logic
    let runShellCheck (cmds : RunCommandList) =
                
        cmds.List
        |> List.iter ( fun cmd ->
            let mutable count = 0   
            let idx = cmd.LineNum
            
            let cmd_list = RunCommand.getAsList cmd
            if Config.DEBUG then printfn $"CMD_LIST @ runShellCheck: \n%A{cmd_list}\n"
                
            cmd_list
            |> List.iter ( fun unit ->
                // Create the tmp. file
                let file_path = $"%s{Config.OUTPUT_DIR}cmd_%s{string count}"
                let tmp_file = openOrCreateRWFile file_path unit    
            
                // Spawn a new shellcheck process ad split output
                let shellcheck_output = shellcheck Config.SHELLCHECK file_path tmp_file    
                let split_output = shellcheck_output.Split(":")    

                closeFile tmp_file
                
                // If shellcheck found something
                if shellcheck_output <> "" then
                    let line = idx + count
                    let problem = stripSheBang unit
                    let char_pos = split_output[2]
                    let response_msg = split_output[4].Trim()
                    
                    printfn $"Around Line %i{line}, char %s{char_pos} \nCmd: '%s{problem}' \nShellcheck Warning: %s{response_msg}\n"
                
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

// Delete all tmp files
let flushTmpFiles  =
    let files = Directory.GetFiles(Config.OUTPUT_DIR)
    for file in files do
        deleteTmpFile file false
    if Config.VERBOSE then printfn $"Files at '%s{Config.OUTPUT_DIR}' deleted successfully.\n"


// Perform the shcellChek on the given commands
let scan (cmds: RunCommandList) =
    // the RUN --mount is a docker specific cmd. Hence we discardd it before shellcheck.
    //@TODO: REFACTOR FLOW HERE:
    let filtered_cmds =
        cmds
        |> RunCommandList.exlcudePrefixedCmds "--mount"
        |> RunCommandList.createRunCommandList
        |> RunCommandList.exlcudePrefixedCmds "--network"
    
    if Config.DEBUG then
        Utils.printHeaderMsg "(SCAN) FILTERED CMDS"
        printfn $"%A{filtered_cmds}\n"
    
    filtered_cmds
    |> List.map prependShebangToCmd
    |> RunCommandList.createRunCommandList 
    |> runShellCheck 
         

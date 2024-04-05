// ================================================
//        SCAN FOR USE OF CERTAIN BINARIES 
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Binaries

open System.Text.RegularExpressions
open Rules.ShellWarn
open System.IO
open Infrastructure

module private Helpers =
    // Takes the list of commands, and splits at the ' '.
    // retruns the commands as a list of a list of the splitted commands 
    let splitCommands (cmd: string list) =
        let delim = [|" "|]
        let rec aux cmd acc =
            match cmd with
            | [] -> acc
            | x :: rest -> aux rest (Utils.splitCmdAt delim x :: acc)
        aux (List.rev cmd) []
   
    // Split an instruction with a line number
    let splitCmd (cmd: Cmd) =
        let line, cmds = cmd.LineNum, cmd.List
        match cmds with
        | Some cmds -> (line, splitCommands cmds)
        | None -> failwith "No cmds to split"
        
    //@TODO: OLD FUNC
    // Split a list of instructions with line number.
    let splitInstructionsOld (cmds: Cmds) =
        let rec aux cmds acc = 
            match cmds with
            | [] -> acc
            | x :: rest ->
                let splitted_cmds = splitCmd x
                aux rest (splitted_cmds :: acc)
        aux cmds.List []
        
    let splitInstructions (cmds: Cmds) =
        cmds.List |> List.map (fun x -> splitCmd x) 
        


module private BinariesInternals =
    // This function Uses regexes to parse the Bash rules.
    // Returns a BashWarn(struct) list
    let extractShellWarningsFromFile (filePath: string) =
        let fileContents = File.ReadAllText(filePath)
        let binRegex = Regex(@"Bin\s*=\s*""([^""]+)""")
        let binariesMatches = binRegex.Matches(fileContents) |> Seq.map (_.Groups[1].Value)
        let codeRegex = Regex(@"Code\s*=\s*""([^""]+)""")
        let msgRegex = Regex(@"Msg\s*=\s*""([^""]+)""")
        
        let code = 
            match codeRegex.Match(fileContents).Groups[1].Value with
            | code when not (Utils.isNullOrWhiteSpace code) -> code
            | _ -> ""
        
        let msg =
            match msgRegex.Match(fileContents).Groups[1].Value with
            | msg when not (Utils.isNullOrWhiteSpace msg) -> msg
            | _ -> ""

        [ for bin in binariesMatches -> { Code = code; Bin = bin; Msg = msg } ]


    // Sequence of all binWarnings  
    let compareWithShellWarnings (directoryPath: string) =
        Directory.GetFiles(directoryPath)
        |> Seq.collect extractShellWarningsFromFile
   
     
    // Print the binary warnings to stdout
    let printShellWarnings (bin: binWarn) =
        printfn $"%s{bin.Code}:\nProblematic Binary: %s{bin.Bin}\nInfo message: %s{bin.Msg}\n"




// =======================================================
//                   Exposed Functions
// =======================================================

open Helpers
    

let scan (rcmds: Cmds )=
    
    
    let rcmds_split = splitInstructions rcmds
    if Config.DEBUG then printfn $"SPLITTED RUNCMDS: \n%A{rcmds_split}\n"
    
    // let base_cmds = takeFirstFromInstructions rcmds_split
    // if Config.DEBUG then printfn $"FIRST FROM LST: \n%A{base_cmds}\n"

    // let dict = instructionToDictLst base_cmds rcmds_split
    // if Config.VERBOSE then printfn $"DICT FROM LSTS: \n%A{dict}\n"
    //
    // //@TODO:
    // // Line numbers 
    // // cheks the base commands:
    // for cmd in base_cmds do
    //     Seq.iter (fun shellWarn ->
    //         match shellWarn with
    //         | _ when cmd = shellWarn.Bin  -> 
    //             if Config.VERBOSE then (printShellWarnings shellWarn)
    //         | _ -> printf ""
    //     ) (compareWithShellWarnings Config.BASH_RULE_DIR)
    //     
    // // check apt command:
    // printf "APT-SCAN CHECK:\n"
    // scanApt (getAptCommands <| dict)



// Loops through the provided binaries from the run cmds to
// looks for matches with known problems.
// let scanOld rcmds =
//     let rcmds_split = splitCommands rcmds
//     if Config.DEBUG then printfn $"SPLITTED RUNCMDS: \n%A{rcmds_split}\n"
//     
//     let base_cmds = takeFirstFromRunCmd rcmds_split
//     if Config.DEBUG then printfn $"FIRST FROM LST: \n%A{base_cmds}\n"
//
//     let dict = binariesToDictLst base_cmds rcmds_split
//     if Config.VERBOSE then printfn $"DICT FROM LSTS: \n%A{dict}\n"
//
//     //@TODO:
//     // Line numbers 
//     // cheks the base commands:
//     for cmd in base_cmds do
//         Seq.iter (fun shellWarn ->
//             match shellWarn with
//             | _ when cmd = shellWarn.Bin  -> 
//                 if Config.VERBOSE then (printShellWarnings shellWarn)
//             | _ -> printf ""
//         ) (compareWithShellWarnings Config.BASH_RULE_DIR)
//         
//     // check apt command:
//     printf "APT-SCAN CHECK:\n"
//     scanApt (getAptCommands <| dict)
        
// ================================================
//        SCAN FOR USE OF CERTAIN BINARIES 
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Binaries

open System.Text.RegularExpressions
open Rules.Misc.AptWarn
open Rules.ShellWarn
open System.IO
open Types

module private Helpers =
    
    /// <summary> Splits the string list at ' ' (spaces) </summary>
    /// <param name="cmd"> The list of commands</param>
    let splitCommands (cmd: string list) =
        let delim = [|" "|]
        let rec aux cmd acc =
            match cmd with
            | [] ->
                match acc with
                | [] -> None
                | _ -> Some acc
            | x :: rest -> aux rest (Utils.splitCmdAt delim x :: acc)
        aux (List.rev cmd) []
   
    
    /// <summary> Split a cmd and update the cmd's internal 'split' list </summary>
    /// <param name="cmd"> A RunCommand to split and update </param>
    let splitCmd (cmd: RunCommand) =
        match RunCommand.getAsList cmd with
        | [] -> failwithf "Error in Binaries.fs: No cmd to split" 
        | x ->
            let split = splitCommands x
            RunCommand.setAsSplitCmd cmd split
      
        
    /// <summary> Splits a RunCommandList and updates the internal field 'asList' of each cmd </summary>
    /// <param name="cmds"> A RunCommandList to update </param>
    let splitCommandsInternalLists (cmds: RunCommandList) : RunCommand list =
        cmds.List |> List.map splitCmd
    
    
    /// <summary> Update line num according to multiline run cmds </summary>
    /// <param name="lst"> A list of (line * cmd_names) to update </param>
    let updateLineNum (lst: (int * string) list) =
        let rec aux lst acc idx =
            match lst with
            | [] ->
                List.rev acc
            | (line, cmd_name) :: rest ->
                let new_line = line + idx
                aux rest ((new_line, cmd_name) :: acc) (idx+1)
        aux lst [] 0
    
    
    /// <summary> Extracts base commands (binary called) from RunCommands list </summary>
    /// <param name="rcmds"> A list of Runcommands to extract from </param>
    let getBaseCommands (rcmds: RunCommand list) = 
        rcmds |> List.fold (fun acc (x:RunCommand) -> (RunCommand.getBaseCommand x)::acc) []
        |> List.rev
        |> List.collect updateLineNum


module private BinariesInternals =
    /// <summary> This function Uses regexes to parse the Bash rules. Returns a BashWarn(struct) list </summary>
    /// <param name="file_path"> The file to scan with the regex </param>
    let extractShellWarningsFromFile (file_path: string) =
        let file_contents = File.ReadAllText(file_path)
        let bin_regex = Regex(@"Binary\s*=\s*""([^""]+)""")
        let matches = bin_regex.Matches(file_contents) |> Seq.map (_.Groups[1].Value)
        let code_regex = Regex(@"ErrorCode\s*=\s*""([^""]+)""")
        let msg_regex = Regex(@"ErrorMsg\s*=\s*""([^""]+)""")
        
        let code = 
            match code_regex.Match(file_contents).Groups[1].Value with
            | code when not (Utils.isNullOrWhiteSpace code) -> code
            | _ -> ""
        
        let msg =
            match msg_regex.Match(file_contents).Groups[1].Value with
            | msg when not (Utils.isNullOrWhiteSpace msg) -> msg
            | _ -> ""

        [ for bin in matches -> { ErrorCode = code; Binary = bin; ErrorMsg = msg } ]


    /// <summary> Sequence of all binWarnings </summary>
    let loadBinaryWarningsIntoMemmory =
        Directory.GetFiles(Config.BASH_RULE_DIR)
        |> Seq.collect extractShellWarningsFromFile
   
     
    /// <summary> Print the binary warnings to stdout </summary>
    /// <param name="warn"> A Shellwarn.BinWarn warning </param>
    /// <param name="line"> A line number </param>
    let printShellWarnings (warn: BinWarn) line =
        Logger.log <| LogBinWarn(line, warn)


module private AptHelpers =
    
    /// <summary> Predicate filter for apt-get update command </summary>
    /// <param name="lst"> A lsit to check </param>
    let isAptUpdate lst =
        List.exists (fun x -> x = "update") lst
    

    /// <summary> Predicate for apt-get install flag </summary>
    /// <param name="lst"> A list of a single (splitted) apt-install command </param>
    let aptHasDashY lst =
        List.contains "-y" lst
                
                
    /// <summary> Predicate for apt-get install flag </summary>
    /// <param name="lst"> A list of a single (splitted) apt-install command </param>
    let aptHasNoInstallRec lst =
        List.contains "--no-install-recommends" lst
    
    

    /// <summary> Unsplit a list </summary>
    /// <param name="rcmds"> A string list of a splitted command </param>
    let unSplitRunCommands (rcmds: string list) =
        List.fold(fun acc x -> acc + x + " ") "" rcmds |> Utils.trimWhitespace
    
    

    /// <summary>
    /// Extracts commands with apt-get as base cmd from the dict.
    /// List is in order of occurance in the dockerfile.
    /// </summary>
    /// <param name="dict"> A custom data structure list of:
    /// ((line *  base_command) * cmd_flags) </param>
    let getAptCommands (dict: ((int * string) * string list) list) =
        let rec aux lst acc =
            match lst with
            | [] -> List.rev acc   
            | ((line, "apt-get"), lst) :: rest -> aux rest (((line, "apt-get"), lst) :: acc)
            | (_, _) :: rest -> aux rest acc
        aux dict []
    
    
    /// <summary> Predicate that pattern matches the apt-get command to see if it contains
    /// the two recommended flags for dockerfiles </summary>
    /// <param name="lst"> A string list of split apt-get command </param>
    let checkAptInstall lst =
        match lst with
        | _ :: "install" :: "-y" :: "--no-install-recommends" :: _ 
        | _ :: "install" :: "-y" :: _ :: ["--no-install-recommends"]
        | _ :: "install" :: "--no-install-recommends" :: "-y" :: _
        | _ :: "install" :: "--no-install-recommends" :: _ :: ["-y"] 
        | _ :: "install" :: _ :: "-y" :: [ "--no-install-recommends" ]
        | _ :: "install" :: _ :: "--no-install-recommends" :: [ "-y" ] -> true
        | _ -> false
    
    
    /// <summary> The print function for apt-get install </summary>
    /// <param name="hasY"> Predicate to check '-y' flag </param>
    /// <param name="hasNoInstall">Predicate to check '--no-install-recommends' flag </param>
    /// <param name="cmd"> A splitted string list of the apt-get command </param>
    /// <param name="line"> A line number </param>
    let printAptWarnings hasY hasNoInstall cmd line =
        let apt_warn =
            if hasNoInstall then
                let warn = shb201
                {
                    ErrorCode = warn.ErrorCode
                    Problem = $"%s{unSplitRunCommands cmd}"
                    ErrorMsg = warn.ErrorMsg
                }
            elif hasY then
                let warn = shb202
                {
                    ErrorCode = warn.ErrorCode
                    Problem = $"%s{unSplitRunCommands cmd}"
                    ErrorMsg = warn.ErrorMsg
                }
            else
                let warn = shb203
                {
                    ErrorCode = warn.ErrorCode
                    Problem = $"%s{unSplitRunCommands cmd}"
                    ErrorMsg = warn.ErrorMsg
                }
                
        Logger.log <| LogAptWarn(line, apt_warn)   

    
    /// <summary> Checks the format of apt-get install and log warnings </summary>
    /// <param name="entry"> A single entry of the ((line *  base_command) * cmd_flags) data structure </param>
    let checkAptOk (entry: (int * string) * string list) = 
        match entry with
        | (line, _), lst ->
            let is_update = isAptUpdate lst
            let is_install = checkAptInstall lst
            
            match (is_update, is_install) with
            | true, _ | _, true -> () 
            | false, _ ->
                let has_y = aptHasDashY lst
                let has_no_install = aptHasNoInstallRec lst
                printAptWarnings has_y has_no_install lst line
        
        
    /// <summary> Scan through the apt-get commands </summary>
    /// <param name="dict"> The ((line *  base_command) * cmd_flags list) list data structure</param>
    let scanApt dict =
        let mutable count = 0
        let rec aux (dict: ((int * string) * string list) list) count =
            match dict with
            | [] -> ()
            | (cmd, lst) :: rest ->
                checkAptOk (cmd, lst)
                aux rest (count + 1)
        aux dict count


// =======================================================
//                   Exposed Functions
// =======================================================
open BinariesInternals
open AptHelpers
open Helpers

/// <summary> The Scan command of the Binaries module </summary>
/// <param name="cmds"> A RunCommandList to scan </param>
let scan (cmds: RunCommandList) =
    
    let split_cmds = splitCommandsInternalLists cmds
    if Config.DEBUG then
        Logger.log <| (LogHeader "BINARIES @ scan: SPLIT RUNCMDS")
        printfn $"\n%A{split_cmds}\n"
    
    let base_cmds =
        split_cmds
        |> getBaseCommands

    
    if Config.DEBUG then
        Logger.log <| (LogHeader "BINARIES @ scan: BASE_CMDS")
        printfn $"\n%A{base_cmds}\n"
    
    let binaryWarnings = loadBinaryWarningsIntoMemmory 
    
    // cheks the base commands:
    for line, cmd in base_cmds do
        Seq.iter (fun shellWarn ->
            match shellWarn with
            | _ when cmd = shellWarn.Binary  -> 
                printShellWarnings shellWarn line
            | _ -> ()
        ) binaryWarnings
        
        
    if Config.DEBUG then
        Logger.log <| (LogHeader "APT-SCAN CHECK:")
        
    // check apt command:
    RunCommandList.collectSplitRuncommandList split_cmds  // 1. Concat each cmds 'split' list into one list
    |> List.zip base_cmds                     // 2. Zip to list of (base_cmd, split_command_lst)
    |> getAptCommands                         // 3. Extract apt-get commands
    |> scanApt                                                      // 4. Run scan 

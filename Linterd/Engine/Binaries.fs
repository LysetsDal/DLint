// ================================================
//        SCAN FOR USE OF CERTAIN BINARIES 
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Binaries

open System.Text.RegularExpressions
open Rules.Misc.AptWarn
open Rules.ShellWarn
open Types
open System.IO

module private Helpers =
    
    // Takes the list of commands, and splits at the ' '.
    // retruns the commands as a list of a list of the splitted commands 
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
   
    
    // Split a cmd and update the cmd's internal 'split' list        
    let splitCmd (cmd: RunCommand) =
        match RunCommand.getAsList cmd with
        | [] -> failwithf $"Error in Binaries.fs: No cmd to split" 
        | x ->
            let split = splitCommands x
            RunCommand.setAsSplitCmd cmd split
      
        
    // Splits a list of commands and updates the internal field 'split' of each cmd 
    let splitCommandsInternalLists (cmds: RunCommandList) : RunCommand list =
        cmds.List |> List.map splitCmd
    
    
    // Update line num according to multiline run cmds
    let updateLineNum (lst: (int * string) list) =
        let rec aux lst acc idx =
            match lst with
            | [] ->
                List.rev acc
            | (line, cmd_name) :: rest ->
                let new_line = line + idx
                aux rest ((new_line, cmd_name) :: acc) (idx+1)
        aux lst [] 0
    
    
    // Extracts base commands from cmd list
    let getBaseCommands (rcmds: RunCommand list) = 
        rcmds |> List.fold (fun acc (x:RunCommand) -> (RunCommand.getBaseCommand x)::acc) []
        |> List.rev
        |> List.collect updateLineNum


module private BinariesInternals =
    // This function Uses regexes to parse the Bash rules.
    // Returns a BashWarn(struct) list
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


    // Sequence of all binWarnings  
    let loadBinaryWarningsIntoMemmory =
        Directory.GetFiles(Config.BASH_RULE_DIR)
        |> Seq.collect extractShellWarningsFromFile
   
     
    // Print the binary warnings to stdout
    let printShellWarnings (warn: BinWarn) line =
        Logger.log Config.LOG_MODE <| LogBinWarn(line, warn)


module private AptHelpers =
    
    // Predicate filter for apt-get update command    
    let isAptUpdate lst =
        List.exists (fun x -> x = "update") lst
    
    // Predicate for apt-get install flag
    let aptHasDashY lst =
        List.contains "-y" lst
                
    // Predicate for apt-get install flag
    let aptHasNoInstallRec lst =
        List.contains "--no-install-recommends" lst
    
    // unsplit a runcommand  
    let unSplitRunCommands (rcmds: string list) =
        List.fold(fun acc x -> acc + x + " ") "" rcmds |> Utils.trimWhitespace
    
    
    // Extracts commands with apt-get as base cmd from the dict .
    // List is in order of occurance in the dockerfile.
    let getAptCommands (dict: ((int * string) * string list) list) =
        let rec aux lst acc =
            match lst with
            | [] -> List.rev acc   
            | ((line, "apt-get"), lst) :: rest -> aux rest (((line, "apt-get"), lst) :: acc)
            | (_, _) :: rest -> aux rest acc
        aux dict []

    
    
    // Predicate that pattern matches the apt-get command to see if it contains
    // the two recommended flags for dockerfiles.
    let checkAptInstall lst =
        match lst with
        | _ :: "install" :: "-y" :: "--no-install-recommends" :: _ 
        | _ :: "install" :: "-y" :: _ :: ["--no-install-recommends"]
        | _ :: "install" :: "--no-install-recommends" :: "-y" :: _
        | _ :: "install" :: "--no-install-recommends" :: _ :: ["-y"] 
        | _ :: "install" :: _ :: "-y" :: [ "--no-install-recommends" ]
        | _ :: "install" :: _ :: "--no-install-recommends" :: [ "-y" ] -> true
        | _ -> false
    
    // The print function for apt-get install.
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
                
        Logger.log Config.LOG_MODE <| LogAptWarn(line, apt_warn)   

    
    // Checks the format of apt-get install and log warnings
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
        
        
    // Scan through the apt-get commands 
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
let scan (cmds: RunCommandList) =
    
    let split_cmds = splitCommandsInternalLists cmds
    if Config.DEBUG then
        Logger.log Config.LOG_MODE <| (LogHeader "BINARIES @ scan: SPLIT RUNCMDS")
        printfn $"\n%A{split_cmds}\n"
    
    let base_cmds =
        split_cmds
        |> getBaseCommands

    
    if Config.DEBUG then
        Logger.log Config.LOG_MODE <| (LogHeader "BINARIES @ scan: BASE_CMDS")
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
        Logger.log Config.LOG_MODE <| (LogHeader "APT-SCAN CHECK:")
        
    // check apt command:
    RunCommandList.collectSplitRuncommandList split_cmds  // 1. Concat each cmds 'split' list into one list
    |> List.zip base_cmds                     // 2. Zip to list of (base_cmd, split_command_lst)
    |> getAptCommands                         // 3. Extract apt-get commands
    |> scanApt                                                      // 4. Run scan 

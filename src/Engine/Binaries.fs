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
            | [] ->
                match acc with
                | [] -> None
                | _ -> Some acc
            | x :: rest -> aux rest (Utils.splitCmdAt delim x :: acc)
        aux (List.rev cmd) []
   
    
    // Split a cmd and update the cmd's internal 'split' list        
    let splitCmd (cmd: Cmd) =
        match Cmd.getList cmd with
        | [] -> failwith "No cmds to split" 
        | x ->
            let split = splitCommands x
            Cmd.setSplit cmd split
      
        
    // Splits a list of commands and updates the internal field 'split' of each cmd 
    let splitCommandsInternalLists (cmds: Cmds) : Cmd list =
        cmds.List |> List.map splitCmd
    
    
    // Extracts base commands from cmd list
    let getBaseCommands (rcmds: Cmd list) = 
        rcmds |> List.fold (fun acc (x:Cmd) -> (Cmd.getBaseCommand x)::acc) []
        |> List.rev


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
    let loadBinaryWarningsIntoMemmory =
        Directory.GetFiles(Config.BASH_RULE_DIR)
        |> Seq.collect extractShellWarningsFromFile
   
     
    // Print the binary warnings to stdout
    let printShellWarnings (bin: binWarn) =
        printfn $"%s{bin.Code}:\nProblematic Binary: %s{bin.Bin}\nInfo message: %s{bin.Msg}\n"


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
    let getAptCommands (dict: (string * string list) list) =
        let rec aux lst acc =
            match lst with
            | [] -> List.rev acc   
            | ("apt-get", lst) :: rest -> aux rest (("apt-get", lst) :: acc)
            | (_, _) :: rest -> aux rest acc
        
        aux dict []

    
    
    // Predicate that pattern matches the apt-get command to see if it contains
    // the two recommended flags for dockerfiles.
    let checkAptInstall lst =
        match lst with
        | _ :: "install" :: "-y" :: "--no-install-recommends" :: _ 
        | _ :: "install" :: "--no-install-recommends" :: "-y" :: _
        | _ :: "install" :: _ :: "-y" :: [ "--no-install-recommends" ]
        | _ :: "install" :: _ :: "--no-install-recommends" :: [ "-y" ] -> true
        | _ -> false
    
    // The print function for apt-get install.
    let printAptWarnings hasY hasNoInstall cmd =
    
        let aptWarn =
            if hasY then
                {
                    Code = "SHB115"
                    Problem = $"%s{unSplitRunCommands cmd}"
                    Msg = "Missing: --no-install-recommends. Use this flag to avoid unnecessary packages being installed on your image."
                }
            elif hasNoInstall then
                {
                    Code = "SHB114"
                    Problem = $"%s{unSplitRunCommands cmd}"
                    Msg = "Missing: -y. Use this flag to avoid the build requiring the user to input 'y', which breaks the image build."
                }
            else
                {
                    Code = "SHB116"
                    Problem = $"%s{unSplitRunCommands cmd}"
                    Msg = "Missing: --no-install-recommends && -y. Use these flags to avoid SHB114 and SHB115."
                }
    
        printfn $"%s{aptWarn.Code}:\nProblem: %s{aptWarn.Problem}\nInfo message: %s{aptWarn.Msg}\n"
    
        
    
    // Checks the format of apt-get install and log warnings
    let checkAptOk (entry: string * string list) = 
        match entry with
        | _, lst ->
            let isAptUpdate = isAptUpdate lst
            let aptInstallOk = checkAptInstall lst
            
            match (isAptUpdate, aptInstallOk) with
            | true, _ | _, true -> printfn "apt-get: ok\n"  
            | false, _ ->
                let hasY = aptHasDashY lst
                let hasNoInstall = aptHasNoInstallRec lst
                printAptWarnings hasY hasNoInstall lst
        
        
    // Scan through the apt-get commands 
    let scanApt dict =
        let mutable count = 0
        let rec aux (dict: (string * string list) list) count =
            
            match dict with
            | [] -> ()
            | x :: rest ->
                printf $"%d{count} "
                checkAptOk x
                aux rest (count + 1)
        aux dict count


// =======================================================
//                   Exposed Functions
// =======================================================
open BinariesInternals
open AptHelpers
open Helpers

let scan (cmds: Cmds) =
    
    let split_cmds = splitCommandsInternalLists cmds
    if Config.DEBUG then printfn $"SPLITTED RUNCMDS: \n%A{split_cmds}\n"
    
   
    let base_cmds = List.concat <| getBaseCommands split_cmds
    if Config.DEBUG then printfn $"BASECMDS LST: \n%A{base_cmds}\n"

    
    let binarieWarnings = loadBinaryWarningsIntoMemmory 
    
    // cheks the base commands:
    for cmd in base_cmds do
        Seq.iter (fun shellWarn ->
            match shellWarn with
            | _ when cmd = shellWarn.Bin  -> 
                if Config.VERBOSE then (printShellWarnings shellWarn)
            | _ -> printf ""
        ) binarieWarnings
        
    // check apt command:
    printfn "APT-SCAN CHECK:\n"
    Cmds.collectSplitRcmds split_cmds // 1. Concat each cmds 'split' list into one list
    |> List.zip base_cmds       // 2. Zip to list of (base_cmd, split_command_lst)
    |> getAptCommands           // 3. Extract apt-get commands
    |> scanApt                                    // 4. Run scan 


        
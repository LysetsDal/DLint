// ================================================
//        SCAN FOR USE OF CERTAIN BINARIES 
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Binaries

open System.Text.RegularExpressions
open Rules.ShellWarn
open System.IO

type splitRunCmd =  string list list              
type dictEntry = string * string list     
type dict = dictEntry list

module private Helpers =
    // Takes the list of run commands, and splits at the ' '.
    // retruns the commands as a list of a list of the splitted commands 
    let splitRunCommands (rcmds: string list) =
        let delim = [|" "|]
        let rec aux rcmds acc =
            match rcmds with
            | [] -> acc
            | x :: rest -> aux rest (Utils.splitCmdAt delim x :: acc)
        aux (List.rev rcmds) []
   

    // Takes the first element of each splitted run cmd, and return them in a list.
    // Used to extract the 'base' of each shell command.  
    let takeFirstFromRunCmd (lst: splitRunCmd) =
        let rec aux lst acc = 
            match lst with
            | [] -> List.rev acc
            | x :: rest -> aux rest (List.head x :: acc)
        aux lst []


    // Construct a Dictionary to use as a lookup table 
    let binariesToDictLst (basecmds: string list) (srcmds: splitRunCmd)  =
        List.zip basecmds srcmds 
 


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
    let getAptCommands (dict: dict) =
        let rec aux dict acc =
            match dict with
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
    let checkAptOk (entry: dictEntry) = 
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
        let rec aux (dict: dict) count =
            
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

// Loops through the provided binaries from the run cmds to
// looks for matches with known problems.
let scan rcmds =
    let rcmds_split = splitRunCommands rcmds
    if Config.DEBUG then printfn $"SPLITTED RUNCMDS: \n%A{rcmds_split}\n"
    
    let base_cmds = takeFirstFromRunCmd rcmds_split
    if Config.DEBUG then printfn $"FIRST FROM LST: \n%A{base_cmds}\n"

    let dict = binariesToDictLst base_cmds rcmds_split
    if Config.VERBOSE then printfn $"DICT FROM LSTS: \n%A{dict}\n"

    //@TODO:
    // Line numbers 
    // cheks the base commands:
    for cmd in base_cmds do
        Seq.iter (fun shellWarn ->
            match shellWarn with
            | _ when cmd = shellWarn.Bin  -> 
                if Config.VERBOSE then (printShellWarnings shellWarn)
            | _ -> printf ""
        ) (compareWithShellWarnings Config.BASH_RULE_DIR)
        
    // check apt command:
    printf "APT-SCAN CHECK:\n"
    scanApt (getAptCommands <| dict)
        
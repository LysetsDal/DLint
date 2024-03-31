// ================================================
//          Linting for Network Interface
// ================================================

[<RequireQualifiedAccess>]
module Linterd.Engine.Network

open System.Text.RegularExpressions
open System.IO
open Rules.MiscWarn

module private NetworkInternals =
    //@TODO: Code Duplication with Mounts.fs
    // This function Uses regexes to parse the netWork rules from their files.
    // Returns a MiscWarn(struct) list
    let extractNetWarnFromFile (filePath: string) =
        let fileContents = File.ReadAllText(filePath)
        let warnRegex = Regex(@"Problem\s*=\s*""([^""]+)""")
        let warnMatches = warnRegex.Matches(fileContents) |> Seq.map (_.Groups[1].Value)
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

        [ for warn in warnMatches -> { Code = code; Problem = warn; Msg = msg } ]


    // Sequence of all netwarning objects 
    let compareWithNetWarnings (directoryPath: string) =
        Directory.GetFiles(directoryPath)
        |> Seq.collect extractNetWarnFromFile
    
    
    // Print the network warning
    let printNetWarnings (err: MiscWarn) =
        printfn $"%s{err.Code}:\nNetwork Warning: %s{err.Problem}\nInfo message: %s{err.Msg}\n"


// =======================================================
//                   Exposed Functions
// =======================================================
open NetworkInternals

// Check if there are any --network=host run commands  
let runNetworkCheck (lst: string list) =
    lst |> Utils.getCmdByPrefix <| "--network=host"
    


// Loops through the provided network cmds to
// looks for matches with known problems.
let scan cmds =    
    for c in cmds do
        Seq.iter (fun x ->
            match x with
            | _ when c = x.Problem || c.Contains x.Problem ->
                if Config.VERBOSE then (printNetWarnings x)
            | _ -> printf ""
        ) (compareWithNetWarnings Config.MISC_RULE_DIR)
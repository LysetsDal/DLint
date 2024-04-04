// ================================================
//          Linting for Network Interface
// ================================================

[<RequireQualifiedAccess>]
module Linterd.Engine.Network

open System.Text.RegularExpressions
open System.IO
open Rules.MiscWarn
open Absyn

module private PortInternal =
        
    // Predicate filter for expose type
    let isExpose (ins: instr) =
        match ins with
        | Expose _ -> true
        | _ -> false
    
    // Returns a list of Expose instructions
    let getExposeInstrs lst =  
        List.filter isExpose lst
    
    // 'Converts' an instruction to type expose
    let instrToExpose (ins: instr) =
        match ins with
        | Expose (idx, e) -> Some e
        | _ -> None

    // Determines if a single port is in range
    let portInRange (port: int) =
        match port with
        | _ when port <= Config.UNIX_MIN_PORT -> false
        | _ when port > Config.UNIX_MAX_PORT -> false
        | _ -> true  
        
    // Determines if both ports in the tuple is in range
    let portTupleInRange (port: int * int) = 
        match port with
        | host, con -> (portInRange host && portInRange con)
    
    
    // Compares the length of the inputed list with the 'in_range' lists.
    // If theyr not equal in size it returns the out-of-range ports.
    let portsNotInRange (lst: int list) = 
        let len = List.length lst
        let in_range = List.filter portInRange lst
        let out_range = List.fold (fun acc x -> if (portInRange x) then acc else x :: acc) [] lst
        if List.length in_range <> len then out_range else []
        
    //@TODO:
    // Handle logging 'nicer'
    // Logs a port warning if its outside UNIX range
    let logPortWarning (e: expose) =
        match e with
        | Port p ->
            if portInRange p then printf ""
            else printfn $"Port %i{p} outside UNIX Range (0 - 65535)\n"
        | PortM (p1, p2) ->
            if portTupleInRange (p1, p2) then printf ""
            elif portInRange p1 then printfn $"Port %i{p2} outside UNIX Range (0 - 65535)\n"
            elif portInRange p2 then printfn $"Port %i{p1} outside UNIX Range (0 - 65535)\n"
            else printfn $"Port %i{p1} and %i{p2} outside UNIX Range (0 - 65535)\n"
        | Ports lst ->
            let out = portsNotInRange lst
            if List.length out = 0 then
                printf ""
            else
                printfn $"Port(s) %A{out} outside UNIX Range (0 - 65535)\n"

    // Logs all port warnings to std out 
    let logAllPortWarnings (instrs: instr list) =
        printfn "INVALID PORTS:"
        (instrs
        |> List.choose instrToExpose  // Filter and convert to expose instances
        |> List.map logPortWarning) |> ignore  // Log warnings
    
    let scanPorts instrs =
        let expCmds = getExposeInstrs instrs 
        logAllPortWarnings expCmds


module private NetworkInternals =
    //@TODO: Code Duplication with Mounts.fs
    // This function Uses regexes to parse the network rule from its file.
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
open PortInternal
// Check if there are any --network=host run commands  
let runNetworkCheck (lst: string list) =
    lst |> Utils.getCmdByPrefix <| "--network=host"
    

// Loops through the provided network cmds to
// looks for matches with known problems.
let scan cmds instr =    
    for c in cmds do
        Seq.iter (fun x ->
            match x with
            | _ when c = x.Problem || c.Contains x.Problem ->
                if Config.VERBOSE then (printNetWarnings x)
            | _ -> printf ""
        ) (compareWithNetWarnings Config.MISC_RULE_DIR)
        
    scanPorts <| instr
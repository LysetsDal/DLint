// ================================================
//          Linting for Network Interface
// ================================================

[<RequireQualifiedAccess>]
module Linterd.Engine.Network

open System.Text.RegularExpressions
open Rules.MiscWarn
open System.IO
open Types
open Absyn

module private PortScanInternals =
        
    // Predicate filter for expose type
    let isExpose (ins: instruction) =
        match ins with
        | Expose _ -> true
        | _ -> false
    
    
    // Returns a list of Expose instructions
    let getExposeInstructions lst =  
        List.filter isExpose lst
    
    
    // 'Converts' an instruction to type expose
    let instructionToExpose (ins: instruction) =
        match ins with
        | Expose (line, e) -> Some (line, e)
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
    let logPortWarning (e: int * expose) =
        match e with
        | l, Port p ->
            if portInRange p then ()
            else printfn $"Around Line %i{l}: Port %i{p} outside UNIX Range (0 - 65535)\n"
        | l, PortM (p1, p2) ->
            if portTupleInRange (p1, p2) then ()
            elif portInRange p1 then printfn $"Around Line %i{l}: Port %i{p2} outside UNIX Range (0 - 65535)\n"
            elif portInRange p2 then printfn $"Around Line %i{l}: Port %i{p1} outside UNIX Range (0 - 65535)\n"
            else printfn $"Around Line %i{l}: Port %i{p1} and %i{p2} outside UNIX Range (0 - 65535)\n"
        | l, Ports lst ->
            let bad_ports = portsNotInRange lst
            if List.length bad_ports = 0 then
                ()
            else
                printfn $"Around Line %i{l}: Port(s) %A{bad_ports} outside UNIX Range (0 - 65535)\n"

    
    // Logs all port warnings to std out 
    let logAllPortWarnings (instrs: instruction list) =
        instrs
        |> List.choose instructionToExpose  // Filter and convert to expose instances
        |> List.map logPortWarning |> ignore    // Log warnings
    
    // Runs the port scan
    let scanPorts instrs =
        let expose_cmds = getExposeInstructions instrs 
        logAllPortWarnings expose_cmds


module private NetworkInternals =
    //@TODO: Code Duplication with Mounts.fs
    // This function Uses regexes to parse the network rule from its file.
    // Returns a MiscWarn(struct) list
    let extractNetWarnFromFile (file_path: string) =
        let file_contents = File.ReadAllText(file_path)
        let warn_regex = Regex(@"Problem\s*=\s*""([^""]+)""")
        let matches = warn_regex.Matches(file_contents) |> Seq.map (_.Groups[1].Value)
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

        [ for warn in matches -> { ErrorCode = code; Problem = warn; ErrorMsg = msg } ]


    // Sequence of all netwarning objects 
    let loadNetWarningsIntoMemory  =
        Directory.GetFiles(Config.MISC_RULE_DIR)
        |> Seq.collect extractNetWarnFromFile
    
    
    // Print the network warning
    let printNetWarnings (line: int) (err: MiscWarn) =
        printfn $"\nAround Line: %i{line}\n%s{err.ErrorCode} Network Warning: '%s{err.Problem}' \nInfo message: %s{err.ErrorMsg}\n"

    
    // Matches a cmd_list with the known network warnings sequence
    let processNetworkWarning (warnings: MiscWarn seq) (line: int) (cmd_list: string list) =
        let rec aux lst = 
            match lst with
            | [] -> ()
            | cmd :: rest -> 
                Seq.iter (fun warning ->
                    match warning with
                    | _ when cmd = warning.Problem || cmd.Contains warning.Problem ->
                        printNetWarnings line warning
                    | _ ->  ()
                    
                ) warnings
                aux rest
        aux cmd_list


    // Check if there are any --network=host run commands  
    let runNetworkScan (cmd_list: RunCommand list) (networkWarnings:MiscWarn seq) =
        cmd_list
        |> List.iter (fun c ->
            let line, base_cmds = c.LineNum, RunCommand.getAsSplitCmd c
            
            base_cmds
            |> List.iter (fun lst ->
                processNetworkWarning networkWarnings <| line <| lst
            )
        )   


// =======================================================
//                   Exposed Functions
// =======================================================
open NetworkInternals
open PortScanInternals


// Loops through the provided network cmds to looks for matches with known problems.
let scan (cmds: RunCommandList) (instrs: instruction list)=
    let filtered_cmds = RunCommandList.includePrefixedCmds "--network" cmds
       
    let cmds_list = RunCommand.removeEmptyEntries <| filtered_cmds
    if Config.DEBUG then printfn $"FILTERED NETCMDS (REMOVED EMPTY): %A{cmds_list}\n"
    
    loadNetWarningsIntoMemory
    |> runNetworkScan cmds_list 
    
    instrs
    |> scanPorts 

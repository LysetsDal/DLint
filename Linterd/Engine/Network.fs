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
        

    /// <summary> Predicate filter for expose isntruction </summary>
    /// <param name="ins"> The isntruction to check </param>
    let isExpose (ins: instruction) =
        match ins with
        | Expose _ -> true
        | _ -> false
    
    
    /// <summary> Returns a list of Expose instructions </summary>
    /// <param name="lst"> The instruction list </param>
    let getExposeInstructions lst =  
        List.filter isExpose lst
    
    

    /// <summary> 'Converts' an instruction to type expose </summary>
    /// <param name="ins"> The instruction to cast to (line * expose) </param>
    let instructionToExpose (ins: instruction) =
        match ins with
        | Expose (line, e) -> Some (line, e)
        | _ -> None

    

    /// <summary> Determines if a single port is in range </summary>
    /// <param name="port"> The port to check </param>
    let portInRange (port: int) =
        match port with
        | _ when port <= Config.UNIX_MIN_PORT -> false
        | _ when port > Config.UNIX_MAX_PORT -> false
        | _ -> true  
        
        
    /// <summary> Determines if both ports in the tuple is in range </summary>
    /// <param name="port"> The PortTuple to check </param>
    let portTupleInRange (port: int * int) = 
        match port with
        | host, con -> (portInRange host && portInRange con)
    
    
    /// <summary>
    /// Compares the length of the inputed list with the 'in_range' lists.
    /// If theyr not equal in size it returns the out-of-range ports.
    /// </summary>
    /// <param name="lst"> The list to check for bad ports </param>
    let portsNotInRange (lst: int list) = 
        let len = List.length lst
        let in_range = List.filter portInRange lst
        let out_range = List.fold (fun acc x -> if (portInRange x) then acc else x :: acc) [] lst
        if List.length in_range <> len then out_range else []
        
        

    /// <summary> Logs a port warning if its outside UNIX range </summary>
    /// <param name="e"> The int * expose instruction to check </param>
    let logPortWarning (e: int * expose) =
        match e with
        | line, Port p ->
            if portInRange p then ()
            else
                Logger.log Config.LOG_AS_CSV <| LogPortWarn(line, p)
            
        | line, PortM (p1, p2) ->
            if portTupleInRange (p1, p2) then ()
            elif portInRange p1 then
                Logger.log Config.LOG_AS_CSV <| LogPortWarn(line, p2)
            elif portInRange p2 then
                Logger.log Config.LOG_AS_CSV <| LogPortWarn(line, p1)
            else
                Logger.log Config.LOG_AS_CSV <| LogPortWarnTuple(line, (p1,p2))
            
        | line, Ports lst ->
            let bad_ports = portsNotInRange lst
            if List.length bad_ports = 0 then
                ()
            else
                Logger.log Config.LOG_AS_CSV <| LogPortsWarnList(line, bad_ports)

    

    /// <summary> Logs all port warnings to stdOut </summary>
    /// <param name="instrs"> The instruction lsit to log warnings from </param>
    let logAllPortWarnings (instrs: instruction list) =
        instrs
        |> List.choose instructionToExpose  // Filter and convert to expose instances
        |> List.map logPortWarning |> ignore              // Log warnings
    
    
    /// <summary> Runs the port scan </summary>
    /// <param name="instrs"> The lsit of instructions to check </param>
    let scanPorts instrs =
        let expose_cmds = getExposeInstructions instrs 
        logAllPortWarnings expose_cmds


module private NetworkInternals =
    
    /// <summary> This function Uses regexes to parse the network rule from its file.
    /// Returns a MiscWarn(struct) list </summary>
    /// <param name="file_path"> The file path to perform the regex match scan on </param>
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



    /// <summary> Sequence of all netwarning objects </summary>
    let loadNetWarningsIntoMemory  =
        Directory.GetFiles(Config.MISC_RULE_DIR)
        |> Seq.collect extractNetWarnFromFile
    
    
    /// <summary> Print the network warning </summary>
    /// <param name="line"> The line number </param>
    /// <param name="warn"> The MiscWarn object </param>
    let printNetWarnings (line: int) (warn: MiscWarn) =
        Logger.log Config.LOG_AS_CSV <| LogNetWarn(line, warn)

    

    /// <summary> Matches a cmd_list with the known network warnings sequence </summary>
    /// <param name="warnings"> Sequence of all MiscWarnings </param>
    /// <param name="line"> The line number (passd on for logging) </param>
    /// <param name="cmd_list"> The list of commands to check </param>
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


    
    /// <summary> Check if there are any --network=host run commands </summary>
    /// <param name="cmd_list"> List of RunCommands </param>
    /// <param name="networkWarnings"> MiscWarn seq</param>
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

/// <summary> Loops through the provided network cmds to looks for matches with known problems </summary>
/// <param name="cmds"> RunCommandList object to scan </param>
/// <param name="instrs"> The instruction liste (used for port scan) </param>
let scan (cmds: RunCommandList) (instrs: instruction list)=
    let filtered_cmds = RunCommandList.includePrefixedCmds "--network" cmds
       
    let cmds_list = RunCommand.removeEmptyEntries <| filtered_cmds
    
    if Config.DEBUG then
        Logger.log Config.LOG_AS_CSV <| (LogHeader "NETWORK @ scan: FILTERED NETCMDS")
        printfn $"\n%A{cmds_list}\n"
    
    loadNetWarningsIntoMemory
    |> runNetworkScan cmds_list 
    
    instrs
    |> scanPorts 

// ================================================
//                Linting for Mounts
// ================================================

module Linterd.Engine.Mounts

open System.Text.RegularExpressions
open System.IO
open Rules.MountWarn
open Absyn


// A predicate filter for Volume instructions
let isVolume (ins: instr) =
    match ins with
    | Volume _ -> true
    | _ -> false


// Unpack a Volume to an instruction
let volumeToList (ins: instr) =
    match ins with
    | Volume (Mnt_pt cmd) -> [cmd]
    | _ -> failwith "Unexpected type"

    
// Extract RUN commands from instruction list
let getVolumeMounts (lst: instr list) : string list =
    lst
    |> List.filter isVolume                     // 1. Predicate
    |> List.collect volumeToList                       // 2. Unwrap instruction


let RunMountToList (ins: instr) =
    match ins with
    | Run cmd -> [cmd]
    | _ -> failwith "Unexpected type"


// Get RUN mount commands from string list
let getRunMounts (lst: string list) : string list =
    lst
    |> Utils.getCmdByPrefix <| "--mount"


let isNullOrWhiteSpace (str: string) =
    str = null || str.Trim() = ""


// This function Uses regexes to parse the mount rules from their files.
// Returns a SensitiveMount(struct) sequence
let extractSensitiveMountsFromFile (filePath: string) =
    let fileContents = File.ReadAllText(filePath)
    let mountRegex = Regex(@"MountPoint\s*=\s*""([^""]+)""")
    let mountMatches = mountRegex.Matches(fileContents) |> Seq.map (_.Groups[1].Value)
    let codeRegex = Regex(@"Code\s*=\s*""([^""]+)""")
    let msgRegex = Regex(@"Msg\s*=\s*""([^""]+)""")
    

    let code = 
        match codeRegex.Match(fileContents).Groups[1].Value with
        | code when not (isNullOrWhiteSpace code) -> code
        | _ -> ""
    
    let msg =
        match msgRegex.Match(fileContents).Groups[1].Value with
        | msg when not (isNullOrWhiteSpace msg) -> msg
        | _ -> ""

    [ for mount in mountMatches -> { Code = code; MountPoint = mount; Msg = msg } ]


// Sequence of all SensitiveMount objects 
let compareWithMountPoints (directoryPath: string) =
    let sensitiveMounts =
        Directory.GetFiles(directoryPath)
        |> Seq.collect extractSensitiveMountsFromFile
    sensitiveMounts


// Print the sensitive mounts
let printMountWarnings (mnt: SensitiveMount) =
    printfn $"%s{mnt.Code}:\nSensetive Mount:%s{mnt.MountPoint}\nInfo message: %s{mnt.Msg}\n"


// Loops through the provided mounts and --mount=types to
// looks for matches with known sensitive mounts.
let scan mounts =
    let sensitiveMountsSeq = compareWithMountPoints Config.RULE_DIR
    
    for mnt in mounts do
        Seq.iter (fun x ->
            match x with
            | _ when x.MountPoint = mnt || mnt.Contains x.MountPoint ->
                if Config.VERBOSE then (printMountWarnings x)
                //@TODO
                // Do soemthing other than printing
            | _ -> printf ""
        ) sensitiveMountsSeq
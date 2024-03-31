// ================================================
//                Linting for Mounts
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Mounts

open System.Text.RegularExpressions
open Rules.MountWarn
open System.IO
open Absyn

module private Helpers =
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

        
    let RunMountToList (ins: instr) =
        match ins with
        | Run cmd -> [cmd]
        | _ -> failwith "Unexpected type"


module private MountInternals =
    // This function Uses regexes to parse the mount rules from their files.
    // Returns a SensitiveMount(struct) list
    let extractSensitiveMountsFromFile (filePath: string) =
        let fileContents = File.ReadAllText(filePath)
        let mountRegex = Regex(@"MountPoint\s*=\s*""([^""]+)""")
        let mountMatches = mountRegex.Matches(fileContents) |> Seq.map (_.Groups[1].Value)
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

        [ for mount in mountMatches -> { Code = code; MountPoint = mount; Msg = msg } ]


    // Sequence of all SensitiveMount objects 
    let compareWithMountPoints (directoryPath: string) =
        Directory.GetFiles(directoryPath)
        |> Seq.collect extractSensitiveMountsFromFile
   
     
    // Print the sensitive mounts
    let printMountWarnings (mnt: SensitiveMount) =
        printfn $"%s{mnt.Code}:\nSensetive Mount:%s{mnt.MountPoint}\nInfo message: %s{mnt.Msg}\n"


// =======================================================
//                   Exposed Functions
// =======================================================
open MountInternals
open Helpers

// Extract RUN commands from instruction list
let getVolumeMounts (lst: instr list) : string list =
    lst
    |> List.filter isVolume                     // 1. Predicate
    |> List.collect volumeToList                       // 2. Unwrap instruction


// Get RUN mount commands from string list
let getRunMounts (lst: string list) : string list =
    lst |> Utils.getCmdByPrefix <| "--mount"


// Loops through the provided mounts and --mount=types to
// looks for matches with known sensitive mounts.
let scan mounts =    
    for mnt in mounts do
        Seq.iter (fun x ->
            match x with
            | _ when mnt = x.MountPoint || mnt.Contains x.MountPoint ->
                if Config.VERBOSE then (printMountWarnings x)
                //@TODO
                // Do soemthing other than printing
            | _ -> printf ""
        ) (compareWithMountPoints Config.MNT_RULE_DIR)
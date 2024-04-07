// ================================================
//                Linting for Mounts
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Mounts

open System.Text.RegularExpressions
open Rules.MountWarn
open Infrastructure
open System.IO
open Absyn

module private Helpers =
    
    // A predicate filter for Volume instructions
    let isVolume (ins: instr) =
        match ins with
        | Volume _ -> true
        | _ -> false

    
    // Unpack a Volume to an tuple (pre-cmd transformation)
    let volumeToTuple (ins: instr) =
        match ins with
        | Volume (line, Mnt_pt mp) -> Cmd.createCmd line mp (Cmd.split mp)
        | _ -> failwith "Not a volume"

            


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
    let loadMountPointsIntoMemory =
        Directory.GetFiles(Config.MNT_RULE_DIR)
        |> Seq.collect extractSensitiveMountsFromFile
   
     
    // Print the sensitive mounts
    let printMountWarnings (line: int) (mnt: SensitiveMount) =
        printfn $"Around Line: %i{line}\n%s{mnt.Code}:\nSensetive Mount:%s{mnt.MountPoint}\nInfo message: %s{mnt.Msg}\n"


// =======================================================
//                   Exposed Functions
// =======================================================
open MountInternals
open Helpers

// Exctracts and transforms volume instructions to a commands type
let rec getVolumeMounts (lst: instr list) =
    lst
    |> List.filter isVolume
    |> List.map volumeToTuple
    |> Cmds.createCmds

// Extracts --mount=type run commands
let getRunMounts (mounts: Cmds) =
    Cmds.filterPrefixInCmds mounts "--mount"
    |> Cmds.createCmds


// Control logic for processing mountpoint warnings.
let processMountWarning (warnings: SensitiveMount seq) (line: int) (mnt_list: string list) =
    let rec aux lst = 
        match lst with
        | [] -> ()
        | x :: xs -> 
            Seq.iter (fun w ->
                match w with
                | _ when x = w.MountPoint || x.Contains w.MountPoint ->
                    if Config.VERBOSE then
                        if Config.VERBOSE then (printMountWarnings line w)
                | _ ->  ()
            ) warnings
            aux xs
    aux mnt_list


// Execute the scan. Needs a list of commands and sensitive mounts
let runMountScan (cmds_list: Cmd list) (warnings: SensitiveMount seq) =
    cmds_list
    |> List.iter (fun c ->
        let line, mounts = c.LineNum, Cmd.getSplit c
        
        mounts
        |> List.iter (fun lst ->
            processMountWarning warnings <| line <| lst
        )
    )


// Loops through the provided volume mounts and --mount=types to
// looks for matches with known sensitive mounts.
let scan (mounts:Cmds) (instructions: instr list) =
    let volume_mounts = getVolumeMounts instructions
    if Config.DEBUG then printfn $"VOLUME mounts: \n%A{volume_mounts}\n"
    
    let run_mounts = getRunMounts mounts
    if Config.DEBUG then printfn $"RUN mounts: \n%A{run_mounts}\n"

    // put the vloume and --mount mounts into one object
    let all_Mounts = Cmds.mergeCmds run_mounts volume_mounts
    
    // Remove empty entries from the list of all mounts
    let cmds_list = Cmd.removeEmptyEntries <| all_Mounts.List
    
    loadMountPointsIntoMemory 
    |> runMountScan cmds_list


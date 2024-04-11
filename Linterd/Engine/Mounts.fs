// ================================================
//                Linting for Mounts
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Mounts

open System.Text.RegularExpressions
open Rules.MountWarn
open System.IO
open Types
open Absyn

module private Helpers =
    
    // A predicate filter for Volume instructions
    let isVolume (ins: instruction) =
        match ins with
        | Volume _ -> true
        | _ -> false

    
    // Unpack a Volume to an tuple (pre-cmd transformation)
    let volumeToTuple (ins: instruction) =
        match ins with
        | Volume (line, Mnt_pt mp) -> RunCommand.createCmd line mp (RunCommand.split mp)
        | _ -> failwith "Error at Mounts @ volumeToTuple: Instruction is not a volume"

            
    // Exctracts and transforms volume instructions to a commands type
    let rec getVolumeMounts (lst: instruction list) =
        lst
        |> List.filter isVolume
        |> List.map volumeToTuple
        |> RunCommandList.createRunCommandList

    
    // Extracts --mount=type run commands
    let getRunMounts (mounts: RunCommandList) =
        mounts
        |> RunCommandList.includePrefixedCmds "--mount"  
        |> RunCommandList.createRunCommandList
        

module private MountInternals =
    // This function Uses regexes to parse the mount rules from their files.
    // Returns a SensitiveMount(struct) list
    let extractSensitiveMountsFromFile (filePath: string) =
        let file_contents = File.ReadAllText(filePath)
        let mount_regex = Regex(@"MountPoint\s*=\s*""([^""]+)""")
        let matches = mount_regex.Matches(file_contents) |> Seq.map (_.Groups[1].Value)
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

        [ for mount in matches -> { ErrorCode = code; MountPoint = mount; ErrorMsg = msg } ]


    // Sequence of all SensitiveMount objects 
    let loadMountPointsIntoMemory =
        Directory.GetFiles(Config.MOUNT_RULE_DIR)
        |> Seq.collect extractSensitiveMountsFromFile
   
    
    // Print the sensitive mounts
    let printMountWarnings (line: int) (mnt: SensitiveMount) =
        Logger.log Config.LOG_MODE <| LogMountWarn(line, mnt)

    
    // Control logic for processing mountpoint warnings.
    let processMountWarning (warnings: SensitiveMount seq) (line: int) (mnt_list: string list) =
        let rec aux lst = 
            match lst with
            | [] -> ()
            | x :: xs -> 
                Seq.iter (fun w ->
                    match w with
                    | _ when x = w.MountPoint || x.Equals w.MountPoint ->
                        printMountWarnings line w
                    | _ ->  ()
                ) warnings
                aux xs
        aux mnt_list
        
        
    // Execute the scan
    // Needs a list of commands and sensitive mounts
    let runMountScan (cmds_list: RunCommand list) (warnings: SensitiveMount seq) =
        cmds_list
        |> List.iter (fun c ->
            let line, mounts = c.LineNum, RunCommand.getAsSplitCmd c
                        
            mounts
            |> List.iter (fun lst ->
                processMountWarning warnings <| line <| lst
            )
        )     


// =======================================================
//                   Exposed Functions
// =======================================================
open MountInternals
open Helpers


// Loops through the provided volume mounts and --mount=types to
// looks for matches with known sensitive mounts.
let scan (mounts:RunCommandList) (instructions: instruction list) =
    let volume_mounts = getVolumeMounts instructions
    
    if Config.DEBUG then
        Logger.log Config.LOG_MODE <| (LogHeader "MOUNTS @ scan: VOLUME MOUNTS")
        printfn $"\n%A{volume_mounts}\n"
    
    let run_mounts = getRunMounts mounts
    
    if Config.DEBUG then
        Logger.log Config.LOG_MODE <| (LogHeader "MOUNTS @ scan: RUN MOUNTS")
        printfn $" \n%A{run_mounts}\n"

    // put the vloume and --mount mounts into one object
    let all_Mounts = RunCommandList.mergeTwoRunCommandLists volume_mounts run_mounts
    
    // Remove empty entries from the list of all mounts
    let cmds_list = RunCommand.removeEmptyEntries <| all_Mounts.List
    
    loadMountPointsIntoMemory 
    |> runMountScan cmds_list

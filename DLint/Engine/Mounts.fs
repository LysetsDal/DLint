// ================================================
//                Linting for Mounts
// ================================================
[<RequireQualifiedAccess>]
module DLint.Engine.Mounts

open System.Text.RegularExpressions
open Rules.MountWarn
open System.IO
open Absyn
open Types

module private Helpers =
    
    /// <summary> A predicate filter for Volume instructions </summary>
    /// <param name="ins"> An instruction to check</param>
    let isVolume (ins: instruction) =
        match ins with
        | Volume _ -> true
        | _ -> false

    
    /// <summary> Unpack a Volume to an tuple (pre-cmd transformation) </summary>
    /// <param name="ins"> An instruction to transform </param>
    let volumeToTuple (ins: instruction) =
        match ins with
        | Volume (line, Mnt_pt mp) -> RunCommand.createCmd line mp (RunCommand.split mp)
        | _ -> failwith "Error at Mounts @ volumeToTuple: Instruction is not a volume"

            

    /// <summary> Exctracts and transforms volume instructions to a commands type </summary>
    /// <param name="lst"> An instrcution lsit to extract them from </param>
    let rec getVolumeMounts (lst: instruction list) =
        lst
        |> List.filter isVolume
        |> List.map volumeToTuple
        |> RunCommandList.createRunCommandList

    
    /// <summary> Extracts --mount=type run commands </summary>
    /// <param name="mounts"> A RunCommandList with all mount commands </param>
    let getRunMounts (mounts: RunCommandList) =
        mounts
        |> RunCommandList.includePrefixedCmds "--mount"  
        |> RunCommandList.createRunCommandList
        

module private MountInternals =
    /// <summary> Uses regexes to parse the mount rules from their files.
    /// Returns a SensitiveMount(struct) list </summary>
    /// <param name="filePath"> The file to scan with the regex </param>
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


    /// <summary> Loads the sequence of all Sensitive Mounts into memory </summary>
    let loadMountPointsIntoMemory =
        Directory.GetFiles(Config.MOUNT_RULE_DIR)
        |> Seq.collect extractSensitiveMountsFromFile

    
    /// <summary> Print the sensitive mounts </summary>
    /// <param name="line"> The line number of the warning </param>
    /// <param name="mnt"> The SensitiveMount warning to log </param>
    let printMountWarnings (line: int) (mnt: SensitiveMount) =
        Logger.log <| LogMountWarn(line, mnt)

    
     
    /// <summary> Control logic for processing mountpoint warnings. </summary>
    /// <param name="warnings"> A list of all SensitiveMounts (sequence) </param>
    /// <param name="line"> The line number of the warning </param>
    /// <param name="mnt_list"> The collected mount list (Run and Volume)</param>
    let processMountWarning (warnings: SensitiveMount seq) (line: int) (mnt_list: string list) =
        let rec aux lst = 
            match lst with
            | [] -> ()
            | x :: xs -> 
                Seq.iter (fun w ->
                    match w with
                    | _ when x = w.MountPoint || x.Contains w.MountPoint ->
                        printMountWarnings line w
                    | _ ->  ()
                ) warnings
                aux xs
        aux mnt_list
        
        
    /// <summary> Execute the mount scan </summary>
    /// <param name="cmds_list"> A list of Runcommands to scan </param>
    /// <param name="warnings"> A list of all SensitiveMounts (sequence) </param>
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


/// <summary> Loops through the provided volume mounts and --mount=types to looks
/// for matches with known sensitive mounts.</summary>
/// <param name="mounts"> RunCommandList to get run --mounts from </param>
/// <param name="instructions"> instruction list to get volume mounts </param>
let scan (mounts:RunCommandList) (instructions: instruction list) =
    let volume_mounts = getVolumeMounts instructions
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "MOUNTS @ scan: VOLUME MOUNTS")
        printfn $"\n%A{volume_mounts}\n"
    
    let run_mounts = getRunMounts mounts
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "MOUNTS @ scan: RUN MOUNTS")
        printfn $" \n%A{run_mounts}\n"

    // put the vloume and --mount mounts into one object
    let all_Mounts = RunCommandList.mergeTwoRunCommandLists volume_mounts run_mounts
    
    // Remove empty entries from the list of all mounts
    let cmds_list = RunCommand.removeEmptyEntries <| all_Mounts.List
    
    loadMountPointsIntoMemory 
    |> runMountScan cmds_list

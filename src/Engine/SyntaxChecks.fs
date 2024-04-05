// ================================================
//          CHEKS ON THE ABSTRACT SYNTAX
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Syntax

module private Helper =
    
    let first a = a
    
// FROM BINARIES.fs
//
// module private AptHelpers =
    //
    // // Predicate filter for apt-get update command    
    // let isAptUpdate lst =
    //     List.exists (fun x -> x = "update") lst
    //
    // // Predicate for apt-get install flag
    // let aptHasDashY lst =
    //     List.contains "-y" lst
    //             
    // // Predicate for apt-get install flag
    // let aptHasNoInstallRec lst =
    //     List.contains "--no-install-recommends" lst
    //
    // // unsplit a runcommand  
    // let unSplitRunCommands (rcmds: string list) =
    //     List.fold(fun acc x -> acc + x + " ") "" rcmds |> Utils.trimWhitespace
    //
    //
    // // Extracts commands with apt-get as base cmd from the dict .
    // // List is in order of occurance in the dockerfile.
    // let getAptCommands (dict: dict) =
    //     let rec aux dict acc =
    //         match dict with
    //         | [] -> List.rev acc   
    //         | ("apt-get", lst) :: rest -> aux rest (("apt-get", lst) :: acc)
    //         | (_, _) :: rest -> aux rest acc
    //     aux dict []
    //
    //
    // // Predicate that pattern matches the apt-get command to see if it contains
    // // the two recommended flags for dockerfiles.
    // let checkAptInstall lst =
    //     match lst with
    //     | _ :: "install" :: "-y" :: "--no-install-recommends" :: _ 
    //     | _ :: "install" :: "--no-install-recommends" :: "-y" :: _
    //     | _ :: "install" :: _ :: "-y" :: [ "--no-install-recommends" ]
    //     | _ :: "install" :: _ :: "--no-install-recommends" :: [ "-y" ] -> true
    //     | _ -> false
    //
    // // The print function for apt-get install.
    // let printAptWarnings hasY hasNoInstall cmd =
    //
    //     let aptWarn =
    //         if hasY then
    //             {
    //                 Code = "SHB115"
    //                 Problem = $"%s{unSplitRunCommands cmd}"
    //                 Msg = "Missing: --no-install-recommends. Use this flag to avoid unnecessary packages being installed on your image."
    //             }
    //         elif hasNoInstall then
    //             {
    //                 Code = "SHB114"
    //                 Problem = $"%s{unSplitRunCommands cmd}"
    //                 Msg = "Missing: -y. Use this flag to avoid the build requiring the user to input 'y', which breaks the image build."
    //             }
    //         else
    //             {
    //                 Code = "SHB116"
    //                 Problem = $"%s{unSplitRunCommands cmd}"
    //                 Msg = "Missing: --no-install-recommends && -y. Use these flags to avoid SHB114 and SHB115."
    //             }
    //
    //     printfn $"%s{aptWarn.Code}:\nProblem: %s{aptWarn.Problem}\nInfo message: %s{aptWarn.Msg}\n"
    //
    //     
    //
    // // Checks the format of apt-get install and log warnings
    // let checkAptOk (entry: dictEntry) = 
    //     match entry with
    //     | _, lst ->
    //         let isAptUpdate = isAptUpdate lst
    //         let aptInstallOk = checkAptInstall lst
    //         
    //         match (isAptUpdate, aptInstallOk) with
    //         | true, _ | _, true -> printfn "apt-get: ok\n"  
    //         | false, _ ->
    //             let hasY = aptHasDashY lst
    //             let hasNoInstall = aptHasNoInstallRec lst
    //             printAptWarnings hasY hasNoInstall lst
    //     
    //     
    // // Scan through the apt-get commands 
    // let scanApt dict =
    //     let mutable count = 0
    //     let rec aux (dict: dict) count =
    //         
    //         match dict with
    //         | [] -> ()
    //         | x :: rest ->
    //             printf $"%d{count} "
    //             checkAptOk x
    //             aux rest (count + 1)
    //     aux dict count
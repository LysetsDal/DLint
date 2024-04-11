// =======================================================
//               TYPES FOR DATA MANIPULATION  
// =======================================================
module Types

open Absyn
open Rules

type store = (int * instruction) list

type LogParams =
    | LogHeader of string
    | LogShellcheckWarn of int * string * string * string
    | LogMiscWarnNoLine of MiscWarn.MiscWarn    // ErrorCode, Problem, ErrorMsg
    | LogMiscWarn of int * MiscWarn.MiscWarn
    | LogBinWarn of int * ShellWarn.BinWarn
    | LogAptWarn of int * ShellWarn.AptWarn
    | LogMountWarn of int * MountWarn.SensitiveMount
    | LogNetWarn of int * MiscWarn.MiscWarn
    | LogPortWarn of int * int
    | LogPortWarnTuple of int * (int * int)
    | LogPortsWarnList of int * int list
    | FlushFiles 




type RunCommand = {
    LineNum: int
    AsString: string
    AsList: string list option
    AsSplitCmd: string list list option
}


module RunCommand =
    // Constructor
    let createCmd line (raw:string) lst =
        { LineNum = line
          AsString = raw.Trim(' ')
          AsList = Some (Utils.split raw)
          AsSplitCmd = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None }
    
    
    // Constructor (takes an additonal list)
    let createCmdWithList line (raw:string) (lst: string list option) =
        { LineNum = line
          AsString = raw.Trim(' ')
          AsList = lst
          AsSplitCmd = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None } 
    
    
    // Getters and Setters
    let getAsString (cmd: RunCommand) =
        cmd.AsString.ToString()
    
    
    let getAsList cmd =
        match cmd.AsList with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    
    let getAsSplitCmd cmd =
        match cmd.AsSplitCmd with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    
    let setAsSplitCmd (cmd:RunCommand) split =
        {
            LineNum = cmd.LineNum
            AsString = cmd.AsString
            AsList = cmd.AsList
            AsSplitCmd = split
        }
    
    let split(str: string) =
        Some (Utils.split str)
        
        
    // Used to remove empty entries from the list in Mounts & Network
    let removeEmptyEntries (cmds:RunCommand list) =
       let rec aux (cmds:RunCommand list) acc =
           match cmds with
           | [] -> List.rev acc
           | x :: rest ->
               match x.AsString with
               | "" -> aux rest acc
               | _ -> aux rest (x:: acc)
       aux cmds []
    
    
    // Filter OUT strings that start with the prefix 
    let excludePrefixedItems (prefix:string) cmd =
        match cmd.AsList with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> not (str.StartsWith prefix)) lst
            |> List.rev

    
    // Filter IN strings that start with the prefix
    let includePrefixedItems (prefix:string) cmd =
        match cmd.AsList with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> (str.StartsWith prefix)) lst
            |> List.rev
             
               
    // Filter OUT strings that start with the prefix, return as new command
    let exlcudePrefixedCmd (prefix: string) (cmd: RunCommand) =
        let filter_list = excludePrefixedItems prefix cmd
        let transformed_cmd = (Utils.reconstructToShellCmd filter_list).Trim('[', ']') 
        createCmd cmd.LineNum transformed_cmd (split transformed_cmd)
        
    
    // Filter IN strings that start with the prefix, return as new command
    let includePrefixedCmd  (prefix: string) (cmd: RunCommand) =
        let filter_list = includePrefixedItems prefix cmd
        let transformed_cmd = (Utils.reconstructToString filter_list).Trim('[', ']')
        match transformed_cmd with
        | "" -> createCmd cmd.LineNum "" None 
        | _ -> createCmd cmd.LineNum transformed_cmd (split transformed_cmd)


        
    // Used to extract the base commands from the splitted commands list    
    let getBaseCommand (cmd: RunCommand) =
        let split_cmd = getAsSplitCmd cmd
        let line = cmd.LineNum
        let rec aux (lst:string list list) acc =
            match lst with
            | [] -> List.rev acc
            | x :: xs ->
                let base_cmd = List.head x
                aux xs ((line, base_cmd) :: acc)
        aux split_cmd []
    
    
    let runCommandToString cmd =
        sprintf $"\nLine: %i{cmd.LineNum}\nRaw: %s{cmd.AsString}\nList: %A{cmd.AsList}\nSplit: %A{cmd.AsSplitCmd}\n"
    
    
    let printList (lst: string list) =
        printfn "[%s]" (String.concat "; " (List.map string lst))
        
        

type RunCommandList = {
    List: RunCommand list
    Length: int
}

module RunCommandList =
    let createRunCommandList lst =
        { List = lst
          Length = List.length lst }

    
    let mergeTwoRunCommandLists (c1:RunCommandList) (c2:RunCommandList) =
        let merged = c1.List @ c2.List
        { List = merged
          Length = List.length merged
        }      
        
        
    // Constructs a new collection of all RunCommandLists' AsSplitCmd fields
    let collectSplitRuncommandList (rcmds: RunCommand list) =
        let rec aux (lst: RunCommand list) acc =
            match lst with
            | [] -> acc
            | x :: xs ->
                let split_list = RunCommand.getAsSplitCmd x
                aux xs (acc @ split_list)
        aux rcmds []
    
    
    // Filter away the commands that start with the prefix
    let exlcudePrefixedCmds (prefix: string) (cmds: RunCommandList) =
        let cmd_list = cmds.List
        List.map (RunCommand.exlcudePrefixedCmd prefix) cmd_list

    
    // Filter a list by a prefix (returns a new list of where it holds)
    let includePrefixedCmds (prefix: string) (cmds: RunCommandList) =
        let cmd_lst = cmds.List
        let res = List.map (RunCommand.includePrefixedCmd prefix) cmd_lst
        res
    
    
    // Used for debug print
    let runCommandListToString cmds =
        let mutable result = ""
        let cmds_list = cmds.List
        for cmd in cmds_list do
            result <- result + RunCommand.runCommandToString cmd
        result
        
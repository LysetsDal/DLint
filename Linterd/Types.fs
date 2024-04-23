// =======================================================
//               TYPES FOR DATA MANIPULATION  
// =======================================================
module Types

open Absyn
open Rules

type store = (int * instruction) list

type LogParams =
    | LogHeader of string
    | LogFileName of string
    | LogShellcheckWarn of int * string * string * string
    | LogMiscWarnNoLine of MiscWarn.MiscWarn   
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
    
    /// <summary> Constructor for RunCommand </summary>
    /// <param name="line"> The line number for the parsed command </param>
    /// <param name="raw"> The cmd as a 'raw string'. This sets the raw which sets the other fields </param>
    /// <param name="lst"> Sets the AsSPlitCmd </param>
    let createCmd line (raw:string) lst =
        { LineNum = line
          AsString = raw.Trim(' ')
          AsList = Some (Utils.split raw)
          AsSplitCmd = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None }
    
    
    /// <summary> Constructor for RunCommand (set the lsit directly) </summary>
    /// <param name="line"> The line number for the parsed command </param>
    /// <param name="raw"> The cmd as a 'raw string'. This sets the AsList and AsSplitCmd </param>
    /// <param name="lst"> Sets the AsSPlitCmd </param>
    let createCmdWithList line (raw:string) (lst: string list option) =
        { LineNum = line
          AsString = raw.Trim(' ')
          AsList = lst
          AsSplitCmd = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None } 
    
    
    /// <summary> Getters and Setters </summary>
    /// <param name="cmd"> Get cmd's raw string </param>
    let getAsString (cmd: RunCommand) =
        cmd.AsString.ToString()
    
    
    /// <summary> Getters and Setters </summary>
    /// <param name="cmd"> Get cmd's 'asList' list </param>
    let getAsList cmd =
        match cmd.AsList with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    
    /// <summary> Getters and Setters </summary>
    /// <param name="cmd"> Get cmd's asSplitCmd list </param>
    let getAsSplitCmd cmd =
        match cmd.AsSplitCmd with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    
    /// <summary> Getters and Setters </summary>
    /// <param name="cmd"> Cmd to update </param>
    /// <param name="split"> List to set </param>
    let setAsSplitCmd (cmd:RunCommand) split =
        {
            LineNum = cmd.LineNum
            AsString = cmd.AsString
            AsList = cmd.AsList
            AsSplitCmd = split
        }
    
    
    /// <summary> Split a raw string into a string list option </summary>
    /// <param name="str"> String to split </param>
    let split(str: string) =
        Some (Utils.split str)
        
        
    /// <summary> Used to remove empty entries from the list in Mounts & Network </summary>
    /// <param name="cmds"> RunCommand list to remove entries from </param>
    let removeEmptyEntries (cmds:RunCommand list) =
       let rec aux (cmds:RunCommand list) acc =
           match cmds with
           | [] -> List.rev acc
           | x :: rest ->
               match x.AsString with
               | "" -> aux rest acc
               | _ -> aux rest (x:: acc)
       aux cmds []
    
    
    /// <summary> Filter OUT strings that start with the given prefix </summary>
    /// <param name="prefix"> The prefix to filter with</param>
    /// <param name="cmd"> The RunCommand with the list to filter through </param>
    let excludePrefixedItems (prefix:string) cmd =
        match cmd.AsList with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> not (str.StartsWith prefix)) lst
            |> List.rev

    
    /// <summary> Filter IN strings that start with the prefix </summary>
    /// <param name="prefix"> The prefix to filter with </param>
    /// <param name="cmd"> The RunCommand with the list to filter through </param>
    let includePrefixedItems (prefix:string) cmd =
        match cmd.AsList with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> (str.StartsWith prefix)) lst
            |> List.rev
             
               

    /// <summary> Filter Out strings that start with the prefix, return new cmd </summary>
    /// <param name="prefix"> The prefix to filter with </param>
    /// <param name="cmd"> The RunCommand with the list to filter through </param>    
    let exlcudePrefixedCmd (prefix: string) (cmd: RunCommand) =
        let filter_list = excludePrefixedItems prefix cmd
        let transformed_cmd = (Utils.reconstructToShellCmd filter_list).Trim('[', ']') 
        createCmd cmd.LineNum transformed_cmd (split transformed_cmd)
        
    
    /// <summary> Filter In strings that start with the prefix, return new cmd </summary>
    /// <param name="prefix"> The prefix to filter with </param>
    /// <param name="cmd"> The RunCommand with the list to filter through </param> 
    let includePrefixedCmd  (prefix: string) (cmd: RunCommand) =
        let filter_list = includePrefixedItems prefix cmd
        let transformed_cmd = (Utils.reconstructToString filter_list).Trim('[', ']')
        match transformed_cmd with
        | "" -> createCmd cmd.LineNum "" None 
        | _ -> createCmd cmd.LineNum transformed_cmd (split transformed_cmd)


        
    /// <summary> Used to extract the base commands (binary name) from the splitted commands list. </summary>
    /// <param name="cmd"> The RunCommand with the list to filter through</param>
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
    
    /// <summary> Return RunCommand as string </summary>
    /// <param name="cmd"> The RunCommand to format as string </param>
    let runCommandToString cmd =
        sprintf $"\nLine: %i{cmd.LineNum}\nRaw: %s{cmd.AsString}\nList: %A{cmd.AsList}\nSplit: %A{cmd.AsSplitCmd}\n"
    

        
        

type RunCommandList = {
    List: RunCommand list
    Length: int
}

module RunCommandList =
    
    /// <summary> Constructor for RunCommandList </summary>
    /// <param name="lst"></param>
    let createRunCommandList lst =
        { List = lst
          Length = List.length lst }

    /// <summary> Merge two RunCommandLists </summary>
    /// <param name="c1"> RunCommandList 1 - the list to append to </param>
    /// <param name="c2"> RunCommandList 2 - the list to append with </param>
    let mergeTwoRunCommandLists (c1:RunCommandList) (c2:RunCommandList) =
        let merged = c1.List @ c2.List
        { List = merged
          Length = List.length merged
        }      
        
        
    /// <summary> Constructs a new collection of all RunCommandLists' AsSplitCmd fields </summary>
    /// <param name="rcmds"> The list of runCommands to collect </param>
    let collectSplitRuncommandList (rcmds: RunCommand list) =
        let rec aux (lst: RunCommand list) acc =
            match lst with
            | [] -> acc
            | x :: xs ->
                let split_list = RunCommand.getAsSplitCmd x
                aux xs (acc @ split_list)
        aux rcmds []
    
    
    /// <summary> Filter away the commands that start with the prefix </summary>
    /// <param name="prefix"> The prefix to filter with </param>
    /// <param name="cmds"> The RunCommandList to filter through </param> 
    let exlcudePrefixedCmds (prefix: string) (cmds: RunCommandList) =
        let cmd_list = cmds.List
        List.map (RunCommand.exlcudePrefixedCmd prefix) cmd_list

    
    /// <summary> Filter in the commands that start with the prefix </summary>
    /// <param name="prefix"> The prefix to filter with </param>
    /// <param name="cmds"> The RunCommandList to filter through </param> 
    let includePrefixedCmds (prefix: string) (cmds: RunCommandList) =
        let cmd_lst = cmds.List
        let res = List.map (RunCommand.includePrefixedCmd prefix) cmd_lst
        res
    
    
    /// <summary> Used for debug prints </summary>
    /// <param name="cmds"> The RunCommandList to print </param>
    let runCommandListToString cmds =
        let mutable result = ""
        let cmds_list = cmds.List
        for cmd in cmds_list do
            result <- result + RunCommand.runCommandToString cmd
        result
        
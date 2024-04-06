module Infrastructure

type Cmd = {
    LineNum: int
    Raw: string
    List: string list option
    mutable Split: string list list option
}

module Cmd = 
    let createCmd line (raw:string) lst =
        { LineNum = line
          Raw = raw.Trim(' ')
          List = Some (Utils.split raw)
          Split = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None }
    
    let createCmdWithLst line (raw:string) (lst: string list option) =
        { LineNum = line
          Raw = raw.Trim(' ')
          List = lst
          Split = 
              match lst with
              | Some lst -> Some (Utils.splitList lst)
              | None -> None } 
    
    
    let getRaw (cmd: Cmd) =
        cmd.Raw.ToString()

    let setRaw(cmd: Cmd) (str: string) =
        {
            LineNum = cmd.LineNum
            Raw = str.Trim(' ')
            List = cmd.List
            Split = cmd.Split 
        }
    
    let getList cmd =
        match cmd.List with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    let setList (cmd:Cmd) list =
        {
            LineNum = cmd.LineNum
            Raw = cmd.Raw
            List = list
            Split = cmd.Split
        }
    
    let getSplit cmd =
        match cmd.Split with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    let setSplit (cmd:Cmd) split =
        {
            LineNum = cmd.LineNum
            Raw = cmd.Raw
            List = cmd.List
            Split = split
        }
    
    let filterList (prefix:string) cmd =
        match cmd.List with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> not (str.StartsWith prefix)) lst
            |> List.rev
    
    let filterListNotStart (prefix:string) cmd =
        match cmd.List with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> (str.StartsWith prefix)) lst
            |> List.rev
            
    let split(cmd: string) =
        Some (Utils.split cmd)
        
    let removeEmptyEntries (cmds:Cmd list) =
       let rec aux (cmds:Cmd list) acc =
           match cmds with
           | [] -> List.rev acc
           | x :: xs ->
               match x.Raw with
               | "" -> aux xs acc
               | _ -> aux xs (x:: acc)
       aux cmds []
       
       
    let filterCmdByPrefix (cmd: Cmd) (prefix: string) =
        let filterList = filterList prefix cmd
        let transformedCmd = (Utils.reconstructToString filterList).Trim('[', ']')
        //@TODO: Remove debug
        // printfn $"TRANSFORMED CMD: %s{transformedCmd}"
        createCmd cmd.LineNum transformedCmd (split transformedCmd)
        
    let filterCmdByNotPrefix (cmd: Cmd) (prefix: string) =
        let filterList = filterListNotStart prefix cmd
        let transformedCmd = (Utils.reconstructToString filterList).Trim('[', ']')
        match transformedCmd with
        | "" -> createCmd cmd.LineNum "" None 
        | _ -> createCmd cmd.LineNum transformedCmd (split transformedCmd)


        // printfn $"TRANSFORMED CMD: %s{transformedCmd}"
        // createCmd cmd.LineNum transformedCmd (split transformedCmd)
        
    // Used to extract the base commands from the splitted commands list    
    let getBaseCommand (cmd: Cmd) =
        let split_cmd = getSplit cmd
        
        let rec aux (lst:string list list) acc =
            match lst with
            | [] -> List.rev acc
            | x :: xs ->
                let base_cmd = List.head x
                aux xs (base_cmd :: acc)
        aux split_cmd []
    
    
    
    
    let cmdToString cmd =
        sprintf $"\nLine: %i{cmd.LineNum}\nRaw: %s{cmd.Raw}\nList: %A{cmd.List}\nSplit: %A{cmd.Split}\n"
    
    let printList (lst: string list) =
        printfn "[%s]" (String.concat "; " (List.map string lst))
        
        
        

type Cmds = {
    List: Cmd list
    Length: int
}

module Cmds =
    let createCmds lst =
        { List = lst
          Length = List.length lst }

    let cmdsToString cmds =
        let mutable result = ""
        let cmdsList = cmds.List
        for cmd in cmdsList do
            result <- result + Cmd.cmdToString cmd
        result
    
    let collectSplitRcmds (rcmds: Cmd list) =
        let rec aux (lst: Cmd list) acc =
            match lst with
            | [] -> acc
            | x :: xs ->
                let split_list = Cmd.getSplit x
                aux xs (acc @ split_list)
        aux rcmds []
    
    // Filter a list by a prefix (returns where it holds)
    let filterPrefixOutOfCmds (cmds: Cmds) (prefix: string) =
        let cmd_list = cmds.List
        List.map (fun cmd -> Cmd.filterCmdByPrefix cmd prefix) cmd_list

    // Filter a list by a prefix (returns where it dosn't hold)
    let filterPrefixInCmds (cmds: Cmds) (prefix: string) =
        let cmd_lst = cmds.List
        let res = List.map (fun cmd -> Cmd.filterCmdByNotPrefix cmd prefix) cmd_lst
        res


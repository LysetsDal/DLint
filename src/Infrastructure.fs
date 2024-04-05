module Infrastructure

type Cmd = {
    LineNum: int
    Raw: string
    List: string list option
    Split: string list list option
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
    
    let getList cmd =
        match cmd.List with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    let getSplit cmd =
        match cmd.Split with
        | None -> failwith "None option in List"
        | Some cmd -> cmd

    let filterList (prefix:string) cmd =
        match cmd.List with
        | None -> []
        | Some lst -> 
            List.filter (fun (str:string) -> not (str.StartsWith prefix)) lst
            |> List.rev
    
    let split(cmd: string) =
        Some (Utils.split cmd)
    
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









// type Cmd =
//     struct
//         val LineNum : int
//         val Raw : string
//         val List : string list option
//         val Split : (string list list) option
//                 
//         new (line, (raw:string), lst: string list option) =
//             {
//                 LineNum = line
//                 Raw = raw.Trim(' ')
//                 List = Some (Utils.split raw)
//                 Split = if lst.IsSome then Some (Utils.splitList lst.Value) else None
//             }
//     
//     
//     member this.getList() =
//         match this.List with
//         | None -> failwith "None option in List"
//         | Some cmd -> cmd
//     
//     member this.getSplit() =
//         match this.Split with
//         | None -> failwith "None option in List"
//         | Some cmd -> cmd
//     
//        
//     member this.filterList(prefix: string) =
//         match this.List with
//         | None -> []
//         | Some lst -> 
//             List.filter (fun (str:string) -> not (str.StartsWith prefix) ) lst
//             |> List.rev
//     
//     override this.ToString() =
//            sprintf $"\nLine: %i{this.LineNum}\nRaw: %s{this.Raw}\nList: %A{this.List}\nSplit: %A{this.Split}\n"
//            
//     end
//     
//
// type Cmds = 
//     struct
//         val List : Cmd list
//         val Length : int
//         
//         new ( lst: Cmd ) =
//             { List = [lst]; Length = List.length [lst] }
//         
//         new ( lst: Cmd list) =
//             { List = lst; Length = List.length [lst] }
//
//     
//         member this.printSplit() =
//             let mutable result = ""
//             let cmds = this.List
//             for cmd in cmds do
//                 let str = sprintf $"%A{cmd.getSplit()}" 
//                 result <- result + str
//         
//         override this.ToString() =
//             let mutable result = ""
//             let cmds = this.List
//             for cmd in cmds do
//                 result <- result + cmd.ToString()
//             result
//     end         
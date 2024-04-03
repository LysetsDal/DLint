// ================================================
//        UTILITY FUNCTIONS FOR LIMNTERD 
// ================================================

module Utils

open System.Text.RegularExpressions

// Print a list 
let printStringList lst (prefix: string) =
    printfn $"\n%s{prefix.ToUpper()} CONTENT: "
    let rec aux lst = 
        match lst with
        | [] -> printfn ""
        | x :: rest ->
            printfn $"[ %s{string x} ]"
            aux rest
    aux lst


// Trim multiple following spaces into one space
let trimWhitespace str =
    let rec aux str acc =
        match str with
        | [] -> List.rev acc
        | ' ' :: (' ' :: _ as tail) -> aux tail acc
        | ' ' :: rest -> aux rest (' ' :: acc)
        | char :: rest -> aux rest (char :: acc)
    aux (List.ofSeq str) []
    |> List.ofSeq
    |> List.map string
    |> String.concat ""


// Used to split a string (envVar) at the '=' (equal)
let splitEnvVar input =
    let pattern = @"(?<!\\)=" // match un-escaped '=' sign
    Regex.Split(input, pattern)
    |> List.ofArray


// Used to split a string (path) at the ' ' (spaces)
let splitAtSpaces input =
    let pattern = @"(?<!\\) " // Match spaces not preceded by a backslash
    Regex.Split(input, pattern)
    |> List.ofArray


// Construct a tuple of the first two elements of a list 
let returnPair lst = 
    match lst with
    | [] -> failwith "Empty list"
    | [_] -> failwith "Only one path"
    | x :: y :: _ -> (x, y)


// Return a Path (Copy-path) (from, to) from a string 
let stringToPath str =
    splitAtSpaces str
    |> returnPair


// Return a Key-Value pair (Key, Val) from a string
let stringToEnvPair str =
    splitEnvVar str
    |> returnPair
    

// Checks if a string is null or whitespace
let isNullOrWhiteSpace (str: string) =
    str = null || str.Trim() = ""


// Split at delimiter(s)
let splitCmdAt (delim: string[]) (cmd: string) =
    List.ofArray (cmd.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))


// Split with stnadard shell delimiters (See Config.fs)
let split (cmd: string)  =
    let delims = [|"&&"; ";"; "|"; "<<"; ">>"|]
    splitCmdAt delims cmd


// Gat a command by a prefix
let  getCmdByPrefix (lst: string list) (prefix: string) =    
    let rec aux (lst: string list) acc =
        match lst with
        | [] -> acc
        | x :: rest ->
            match x.StartsWith prefix with
            | true ->  aux rest (x :: acc)
            | _ ->  aux rest acc
    aux (List.rev lst) []
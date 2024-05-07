// ================================================
//        UTILITY FUNCTIONS FOR LIMNTERD 
// ================================================

module Utils

open System.Text.RegularExpressions

/// <summary> Trim multiple following spaces into one space </summary>
/// <param name="str"></param>
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


/// <summary> Construct a spaceseparated string from the string list </summary>
/// <param name="lst"> The string list to construct from </param>
let reconstructToString (lst: string list) =
    (String.concat " " lst).Trim()


/// <summary> Construct a '&&' separated shellcmd as string </summary>
/// <param name="lst"> The string list to construct the command from </param>
let reconstructToShellCmd (lst: string list) =
    (String.concat " && " lst).Trim()
    


/// <summary> Used to split a string (envVar) at the '=' </summary>
/// <param name="input"> The env var to split </param>
let splitEnvVar input =
    let pattern = @"(?<!\\)=" // match un-escaped '=' sign
    Regex.Split(input, pattern)
    |> List.ofArray



/// <summary> Used to split a string (path) at the ' ' (space) </summary>
/// <param name="input"> Split string at spaces </param>
let splitAtSpaces input =
    let pattern = @"(?<!\\) " // Match spaces not preceded by a backslash
    Regex.Split(input, pattern)
    |> List.ofArray



/// <summary> Split list of strings at spaces </summary>
/// <param name="input"> The list to split each string element of </param>
let splitListAtSpaces (input: string list) : string list list =
    List.fold (fun acc x -> splitAtSpaces x :: acc) [] input



/// <summary> Split at delimiter(s) </summary>
/// <param name="delim"> String array of delimiters </param>
/// <param name="cmd"> The string to split at delim </param>
let splitCmdAt (delim: string[]) (cmd: string) =
    List.ofArray (cmd.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))


/// <summary> Split a string with standard shell delimiters. Used to split a multiline or
/// '&& separate' shellcommand into single shellcommands </summary>
/// <param name="cmd"> The string to split </param>
let split (cmd: string)  =
    let delims = [|"&&"; ";"; "<<"; ">>"|]
    splitCmdAt delims cmd
    |> List.map (fun (x:string) -> x.Trim(' '))


/// <summary> Splits a list at the spaces, and removes fsharp list brackets </summary>
/// <param name="input"> The list to split </param>
let splitList (input: string list) =
    let first_split = List.map (fun (s:string) -> s.Trim('[', ']')) input
    List.rev (splitListAtSpaces first_split)
        
        
/// <summary> Construct a tuple of the first two elements of a list </summary>
/// <param name="lst"> The list to construct a tuple from </param>
let returnPair lst = 
    match lst with
    | [] -> failwith "UTILS @ returnPair: Empty list provided (needs two elements)"
    | [_] -> failwith "UTILS @ returnPair: Only one element provided (needs two)"
    | x :: y :: _ -> (x, y)



/// <summary> Return a Path from a string </summary>
/// <param name="str"> String to get a copy path: (from, to) from </param>
let stringToPath str =
    splitAtSpaces str
    |> returnPair



/// <summary> Return a Key-Value pair </summary>
/// <param name="str"> The string to attempt to split into (Key, value) </param>
let stringToEnvPair str =
    splitEnvVar str
    |> returnPair
    

/// <summary> Checks if a string is null or whitespace </summary>
/// <param name="str"> String to check </param>
let isNullOrWhiteSpace (str: string) =
    str = null || str.Trim() = ""


/// <summary> Get a command by a prefix (returns a list of mathces) </summary>
/// <param name="lst"> List to scan through </param>
/// <param name="prefix"> Prefix to filter IN </param>
let getCmdByPrefix (lst: string list) (prefix: string) =    
    let rec aux (lst: string list) acc =
        match lst with
        | [] -> acc
        | x :: rest ->
            match x.StartsWith prefix with
            | true ->  aux rest (x :: acc)
            | _ ->  aux rest acc
    aux (List.rev lst) []

module Utils

open System.Text.RegularExpressions

(* Trim multiple following spaces into one space *)
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

(* Used to split a string (envVar) at the '=' (equal) *)
let splitEnvVar input =
    let pattern = @"(?<!\\)=" // match un-escaped '=' sign
    Regex.Split(input, pattern)
    |> List.ofArray

(* Used to split a string (path) at the ' ' (spaces) *)
let splitAtSpaces input =
    let pattern = @"(?<!\\) " // Match spaces not preceded by a backslash
    Regex.Split(input, pattern)
    |> List.ofArray

(* Construct a tuple of the first two elements of a list*)
let returnPair lst = 
    match lst with
    | [] -> failwith "Empty list"
    | [_] -> failwith "Only one path"
    | x :: y :: _ -> (x, y)

(* Return a Path (Copy-path) (from, to) from a string *)
let stringToPath str =
    splitAtSpaces str
    |> returnPair

let stringToEnvPair str =
    splitEnvVar str
    |> returnPair
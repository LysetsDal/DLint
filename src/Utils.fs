module Utils

open System.Text.RegularExpressions

(* Trim multiple following spaces into one space *)
let trimWhitespace (str: string) =
    let rec aux (str: char list) (acc: char list) =
        match str with
        | [] -> List.rev acc
        | ' ' :: (' ' :: rest as tail) -> aux tail acc
        | ' ' :: rest -> aux rest (' ' :: acc)
        | char :: rest -> aux rest (char :: acc)
    aux (List.ofSeq str) []
    |> List.ofSeq
    |> List.map string
    |> String.concat ""
    

(* Used to split a string (path) at the ' ' (spaces) *)
let splitAtSpaces (input: string) =
    let pattern = @"(?<!\\) " // Negative lookbehind assertion to match spaces not preceded by a backslash
    Regex.Split(input, pattern)
    |> List.ofArray

(* Construct a tuple of the first two elements of a list*)
let returnPair lst = 
    match lst with
    | [] -> failwith "Empty list"
    | [x] -> failwith "Only one path"
    | x :: y :: _ -> (x, y)

(* Return a CPath (Copy-path) (from, to) from a string *)
let stringToCPath str =
    splitAtSpaces str
    |> returnPair

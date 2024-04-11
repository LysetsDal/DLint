// ================================================
//                 MAIN FUNCTION 
// ================================================
[<RequireQualifiedAccess>]
module Program

open System.Text
open FSharp.Text.Lexing
open System.IO
open System
open Linterd
open Engine.Interp

let prepareFile file =
    let file_name = string file
    let in_memory = StringBuilder()

    // Read the content of the file into the in-memory StringBuilder
    use reader = new StreamReader(file_name)
    while not reader.EndOfStream do
        in_memory.Append(reader.ReadLine()) |> ignore
        in_memory.AppendLine() |> ignore

    // Append "EOF" to indicate the end of the file
    in_memory.Append("EOF\n") |> ignore
    
    // Convert the accumulated content to a string
    printfn $"File Read: %s{file_name}\n"
    in_memory.ToString()


// Parses the Dockerfile into the abstract syntax tree
let parseDockerfile file_name file_stream =
    let lexbuf = LexBuffer<char>.FromString file_stream
    let docker_file =
        try
            DPar.Main DLex.Token lexbuf
        with
            | err ->
                 let pos = lexbuf.EndPos 
                 failwithf $"%s{err.Message} in file %s{file_name} near line %d{pos.Line+1}, \
                 column %d{pos.Column}, last parsed: '%s{String(lexbuf.Lexeme)}' \n"
    docker_file
    

[<EntryPoint>]
let Main args =
    for arg in args do
        arg 
        |> prepareFile  
        |> parseDockerfile arg
        |> run
    0
    
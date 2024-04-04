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
    let filename = string file
    let inMemoryFile = StringBuilder()

    // Read the content of the file into the in-memory StringBuilder
    use reader = new StreamReader(filename)
    while not reader.EndOfStream do
        inMemoryFile.Append(reader.ReadLine()) |> ignore
        inMemoryFile.AppendLine() |> ignore

    // Append "EOF" to indicate the end of the file
    inMemoryFile.Append("EOF\n") |> ignore
    
    // Convert the accumulated content to a string
    printfn $"File Read: %s{filename}"
    inMemoryFile.ToString()

// Runs the Dockerfile
let parseDockerfile fname fstream =
    let lexbuf = LexBuffer<char>.FromString fstream
    let dfile =
        try
            DPar.Main DLex.Token lexbuf
        with
            | err ->
                 let pos = lexbuf.EndPos 
                 failwithf $"%s{err.Message} in file %s{fname} near line %d{pos.Line+1}, \
                 column %d{pos.Column}, last parsed: '%s{String(lexbuf.Lexeme)}' \n"
    dfile
    

[<EntryPoint>]
let Main args =
    for arg in args do
        arg 
        |> prepareFile  
        |> parseDockerfile arg
        |> run
    0
    
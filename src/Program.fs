// ================================================
//                 MAIN FUNCTION 
// ================================================

module Program

open FSharp.Text.Lexing
open System.IO
open System
open Interp

let parseDockerfile file =
    let filename = string file
    if Config.VERBOSE then
        printfn $"File Read: %s{filename}"
        
    use reader = new StreamReader(filename)
    let lexbuf = LexBuffer<char>.FromTextReader reader
    let dfile =
        try
            DPar.Main DLex.Token lexbuf
        with
            | err ->
                 let pos = lexbuf.EndPos 
                 failwithf $"%s{err.Message} in file %s{filename} near line %d{pos.Line+1}, \
                 column %d{pos.Column}, last parsed: '%s{String(lexbuf.Lexeme)}' \n"
    dfile
    
    
[<EntryPoint>]
let Main args =
    for arg in args do
        parseDockerfile arg |>
        run
    0
    
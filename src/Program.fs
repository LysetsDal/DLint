module Program

open System
open System.IO
open FSharp.Text.Lexing
open 


[<EntryPoint>]
let Main argv =
    let filename = string argv
    use reader = new StreamReader(filename)
    let lexbuf = LexBuffer<char>.FromTextReader reader
    let dfile =
        try
            DPar.Main DLex.Token lexbuf
        with
            | err -> let pos = lexbuf.EndPos 
                     failwithf $"%s{err.Message} in file %s{filename} near line %d{pos.Line+1}, \
                     column %d{pos.Column}, last parsed: '%s{new String(lexbuf.Lexeme)}' \n"
    
    Interp.run dfile |> ignore
    0
    

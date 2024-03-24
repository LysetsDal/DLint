module Program

open System
open System.IO
open FSharp.Text.Lexing

let fromString = Parse.fromString
let fs = Parse.fromString

let fromFile = Parse.fromFile
let ff = Parse.fromFile

let interp = Interp.run
 
[<EntryPoint>]
let Main args =
    
    for arg in args do
        let filename = string arg
        printfn $"%s{filename}"
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
    

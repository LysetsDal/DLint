// ================================================
//                 MAIN FUNCTION 
// ================================================
[<RequireQualifiedAccess>]
module Program

open Linterd.Engine.Interp
open FSharp.Text.Lexing
open System.Text
open System.IO
open System

/// <summary> Prepare the Dockerfile for parsing. This step reads the file into
/// a stringbuidler and appends a EOF character to ensure proper file termination.</summary>
/// <param name="file">Dockerfile</param>
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
    printfn $"\nFile Read: %s{file_name}\n"
    in_memory.ToString()


/// <summary> Parses the Dockerfile into the abstract syntax tree.</summary>
/// <param name="file_name">Dockerfile path</param>
/// <param name="file_stream">Filestream is the file as a string</param>
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

/// <summary>Set log mode to csv format</summary>
let setLogModeTrue () =
    Config.LOG_AS_CSV <- true

/// <summary>Set log mode to normal format</summary>
let setLogModeFalse () =
    Config.LOG_AS_CSV <- false

/// <summary> Check if the arg list contains the --log-mode=csv flag.</summary>
/// <param name="args">List of arguments</param>
let argsContainLogCSVFlag args =
    args |> Array.exists (fun arg -> arg = "--log-mode=csv")

/// <summary> Check if the arg list contains the --log-mode=normal flag.</summary>
/// <param name="args">List of arguments</param>
let argsContainLogNormalFlag args =
    args |> Array.exists (fun arg -> arg = "--log-mode=normal")


/// <summary>Removing flags so only files to be scanned remain.</summary>
/// <param name="args">List of arguments</param>
/// <param name="flag">flag to remove</param>
let removeLogModeFlag args flag =
    args |> Array.filter (fun arg -> arg <> flag)



/// <summary> Entrypoint of Linterd </summary>
/// <param name="args"> List of dockerfiles and runtime flags </param>
[<EntryPoint>]
let main args =
    let argList =
        if argsContainLogCSVFlag args then
            setLogModeTrue ()
            removeLogModeFlag args "--log-mode=csv"
        elif argsContainLogNormalFlag args then
            setLogModeFalse ()
            removeLogModeFlag args "--log-mode=normal"
        else
            args

    Array.iter (fun arg ->
        arg
        |> prepareFile
        |> parseDockerfile arg
        |> run) argList
    0

    
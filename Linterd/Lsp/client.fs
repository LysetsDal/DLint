module LSP.LSP

// #r "nuget:FSharp.Json"

open System.Text.Json.Nodes
open System.Text
open FSharp.Json
open System.IO
open System.Diagnostics

type ShellcheckJson = {
    content: string
}

type LspJson = {
    jsonrpc: string
    id: string
    method: string
    lparams: string
}

let raw = "Around line: 42\nSHB114:\nProblematic Binary: cd\nInfo message: You should use WORKDIR instead of cluttering run instructions with 'cd' commands, which are hard to read, troubleshoot, and maintain."

let fillJson (rpc:string) (id:string) (param:string) (content:string) =
    sprintf 
        // ReSharper disable once FSharpInterpolatedString
        "{ \"jsonrpc\": \"%s\",
          \"id\": \"%s\",
          \"method\": \"textDocument/completion\",
          \"params\": {
              \"content\": \"%s\"
          }
        }" rpc id content


let test: LspJson = {
    jsonrpc = "2.0"
    id = "1.0"
    method = "textDocument/completion"
    lparams = "" 
}
    
// used 
let encodeMsg (msg:LspJson) =
    let content = Json.serialize msg
    sprintf $"Content-Length: %i{content.Length}\r\n\r\n%s{content}"
    
        
let (bytes: byte array) = Encoding.UTF8.GetBytes(encodeMsg test)


let split (msg: string) =
    let delim = [|"\r\n\r\n";|]
    let split = List.ofArray (msg.Split(delim, System.StringSplitOptions.RemoveEmptyEntries))
    // printfn $"Split: %A{split}"
    (split[0], split[1])


let decode (msg: byte array) =
    let received = Encoding.UTF8.GetString(msg)
    let header, content = split received
    
    let cutAt = String.length "Content-Length: "
    let content_length = int header[cutAt..]
    
    
    // Parse the JSON string
    // let json_obj = JsonValue.Parse(content)
    // let json_value = json_obj.AsObject()
   
    // printfn $"JSON OBJ: %s{json_obj.ToString()}"
    
    content_length, msg[content_length..]
   

// Scanner

type Scanner(stream: StreamReader) =
    let mutable currentLine = ""
    let mutable scannerDisposed = false
    let mutable hasNext = false
    
    let readNextLine () =
        if not scannerDisposed then
            match stream.ReadLine() with
            | null -> scannerDisposed <- true
            | line -> currentLine <- line 
    
    
    // Function to read the next token
    let readNextToken () =
        let rec readToken acc =
            match acc with
            | "" when not scannerDisposed -> 
                readNextLine()
                readToken currentLine
            | "" -> None
            | _ ->
                match acc.IndexOfAny [| ' '; '\t'; '\n'; '\r' |] with
                | -1 -> // No whitespace found, return the entire string as token
                    Some (acc, "")
                | index -> // Found whitespace, split into token and rest
                    let token = acc.Substring(0, index)
                    let rest = acc.Substring(index + 1)
                    Some (token, rest)
        
        readToken currentLine

    member this.Dispose() =
        stream.Dispose()


    member this.ScanTokens() =
        seq {
            while not scannerDisposed do
                match readNextToken() with
                | Some (token, rest) ->
                    yield token
                    currentLine <- rest
                | None -> yield! Seq.empty
        }
        
    member this.HasNext() =
        if not hasNext then
            let peekStream = new StreamReader(stream.BaseStream)
            let nextLine = peekStream.ReadLine()
            hasNext <- nextLine <> null
            peekStream.Close()
        hasNext

    member this.Text() =
        currentLine
        

let main_loop =
    let proc = new Process()
    let ctx = ProcessStartInfo("./LSP/lsp_tmp")
    ctx.RedirectStandardInput <- true
    ctx.RedirectStandardOutput <- true
    ctx.UseShellExecute <- false
    
    use stdOut = proc.StandardOutput
    use stdIn = proc.StandardInput
    let scanner = Scanner(stdOut)
    
    
    stdIn.WriteLine("Hello World!")
    stdIn.Close()
    
    ctx.RedirectStandardInput <- false
    ctx.RedirectStandardOutput <- false
    let thread = proc.Start()
    
    // Process each line from the process output
    for token in scanner.ScanTokens() do
        printfn $"Token: %s{token}"
    
    
    let msg = scanner.Text()
    printfn $"Msg: %s{msg}"

    
    
   
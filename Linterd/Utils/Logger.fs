// =======================================================
//                  CENTRALIZED LOGGING 
// =======================================================
[<RequireQualifiedAccess>]
module Logger

open Rules.MiscWarn
open Types

// LOGGING STORE 
/// <summary> Print the content of the store (in-memory AST instruction list)</summary>
/// <param name="s"> The store </param>
let printStore (s: store) =
    let rec aux s =
        match s with
        | [] -> printfn ""
        | (idx, instr) :: rest -> 
            printfn $"%d{idx} = [ %A{instr} ]; "
            aux rest
    aux s
    

/// <summary> Print a header with a message </summary>
/// <param name="msg"> The message to include in the header </param>
let printHeaderMsg msg =
    let msgLength = String.length msg
    let paddingLength = (70 - msgLength) / 2  // Assuming the total width is 70 characters
    
    let paddedMsg = 
        sprintf "%*s%s%*s" paddingLength " " msg paddingLength " "
    
    printfn "======================================================================"
    printfn $"%s{paddedMsg}"
    printfn "======================================================================"


/// <summary>
///  Handles all info and warning logging.
///  Two modes of logging: human-readable and CSV (controlled in Config.fs)
///  Note. Errors are handled locally in the src .fs files 
/// </summary>
let log = function
    | LogHeader msg -> printHeaderMsg msg
    | LogFileName name ->
        if not Config.LOG_AS_CSV then printfn $"\nFile Read: %s{name}\n"
        else printfn $"\nFile Read,%s{name}\n"

    | LogShellcheckWarn (line, char, problem, msg) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line}, char %s{char} \nShellcheck Warning: \nProblem: '%s{problem}' \nInfo message: %s{msg}\n"
        else printfn $"Around Line,%i{line}\nchar,%s{char}\nShellcheckWarn\nProblem,'%s{problem}'\nInfo message,\"%s{msg}\"\n"
    
    | LogMiscWarnNoLine warn ->
        if not Config.LOG_AS_CSV then printf $"%s{warn.ErrorCode} \nProblem: %s{warn.Problem} \nInfo Message: %s{warn.ErrorMsg}\n"
        else printf $"ErrorCode,%s{warn.ErrorCode}\nProblem,%s{warn.Problem}\nInfo Message,\"%s{warn.ErrorMsg}\"\n"
    
    | LogMiscWarn (line, warn) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line} \n%s{warn.ErrorCode}: \nProblem: %s{warn.Problem} \nInfo Message: %s{warn.ErrorMsg}\n"
        else printfn $"Around Line,%i{line}\nErrorCode,%s{warn.ErrorCode}\nProblem,%s{warn.Problem}\nInfo Message,\"%s{warn.ErrorMsg}\"\n"
    
    | LogBinWarn (line, bin) -> 
        if not Config.LOG_AS_CSV then printfn $"Around line: %d{line} \n%s{bin.ErrorCode}:\nProblematic Binary: %s{bin.Binary}\nInfo message: %s{bin.ErrorMsg}\n"
        else printfn $"Around line,%d{line}\nErrorCode,%s{bin.ErrorCode}\nProblematic Binary,%s{bin.Binary}\nInfo message,\"%s{bin.ErrorMsg}\"\n"
    
    | LogAptWarn (line, apt) ->
        if not Config.LOG_AS_CSV then printfn $"Around line: %d{line} \n%s{apt.ErrorCode}:\nProblem: %s{apt.Problem}\nInfo message: %s{apt.ErrorMsg}\n"
        else printfn $"Around line,%d{line}\nErrorCode,%s{apt.ErrorCode}\nProblem,%s{apt.Problem}\nInfo message,\"%s{apt.ErrorMsg}\"\n"
    
    | LogMountWarn (line, mnt) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line} \n%s{mnt.ErrorCode}: \nSensetive Mount:%s{mnt.MountPoint} \nInfo message: %s{mnt.ErrorMsg}\n"
        else printfn $"Around Line,%i{line}\nErrorCode,%s{mnt.ErrorCode}\nSensetive Mount,%s{mnt.MountPoint}\nInfo message,\"%s{mnt.ErrorMsg}\"\n"
    
    | LogNetWarn (line, warn) ->
        if not Config.LOG_AS_CSV then printfn $"\nAround Line: %i{line} \n%s{warn.ErrorCode} Network Warning: '%s{warn.Problem}' \nInfo message: %s{warn.ErrorMsg}\n"
        else printfn $"Around Line,%i{line}\nErrorCode,%s{warn.ErrorCode}\nNetwork Warning,'%s{warn.Problem}'\nInfo message,\"%s{warn.ErrorMsg}\"\n"
    
    | LogPortWarn (line, port) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line} \nPort %i{port} outside UNIX Range (0 - 65535)\n"
        else printfn $"Around Line,%i{line}\nPortWarning\nInfo message,\"Port %i{port} outside UNIX Range (0 - 65535)\"\n"
    
    | LogPortWarnTuple (line, (p1, p2)) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line} \nPort %i{p1} and %i{p2} outside UNIX Range (0 - 65535)\n"
        else printfn $"Around Line,%i{line}\nPortWarning\nInfo message,\"Port %i{p1} and %i{p2} outside UNIX Range (0 - 65535)\"\n"
    
    | LogPortsWarnList (line, bad_ports) ->
        if not Config.LOG_AS_CSV then printfn $"Around Line: %i{line} \nPort(s) %A{bad_ports} outside UNIX Range (0 - 65535)\n"
        else printfn $"Around Line,%i{line}\nPortWarning\nInfo message,\"Port(s) %A{bad_ports} outside UNIX Range (0 - 65535)\"\n"
    
    | FlushFiles ->
        if not Config.LOG_AS_CSV then printfn $"Files at '%s{Config.OUTPUT_DIR}' deleted successfully.\n"
        else printfn "\n"    
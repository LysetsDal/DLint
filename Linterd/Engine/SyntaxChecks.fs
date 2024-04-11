// ================================================
//          CHEKS ON THE ABSTRACT SYNTAX
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Syntax

open Absyn

module private UserScanInternals =
    open Rules.Misc.UserWarns
    
    let isUser (ins: instruction) =
        match ins with
        | User _ -> true
        | _ -> false

    let getUserInstructions lst =
        List.filter isUser lst

    let instructionToTuple (ins: instruction) =
        match ins with
        | User (line, (Some name, Some uid)) -> (line, (name, uid))
        | User (line, (Some name, None)) -> (line, (name, -1))
        | User (line, (None, Some uid)) -> (line, ("", uid))
        | _ -> (-1, ("", -1))

    let instructionsToTuples (instrs: instruction list) =
        List.map instructionToTuple instrs
    
    
    let userNameIsRoot (user: int * (string * int)) =
        match user with
        | _, (name, _) ->
            match name.ToLower() with
            | "root" -> true
            | _ -> false
            
            
    let userIdIsRoot (user: int * (string * int)) =
        match user with
        | _, (_, id) ->
            match id with
            | 0 -> true
            | _ -> false
    
    let userIsRoot (user: int * (string * int)) =
        (userNameIsRoot user || userIdIsRoot user) 
        
    
    let userListEmpty users =
        match List.length users with
        | 0 ->
            let warn = userWarn101
            printf $"%s{warn.ErrorCode} \nProblem: %s{warn.Problem} \nInfo Messagw:%s{warn.ErrorMsg}"
            users
        | _ -> users

    
    let lastUserRoot (users: (int * (string * int)) list) =
        match List.rev users with
        | (line, (name, uid)) :: _ ->
            let warn = userWarn100
            if userIsRoot (line, (name, uid)) then
                printfn $"Around Line %i{line} \n%s{warn.ErrorCode} \nProblem: %s{warn.Problem} \nInfo Messagw:%s{warn.ErrorMsg}"
            else
                ()
        | _ -> ()         
        

    
open UserScanInternals
let scan (instr: instruction list) =       
    let user_instructions = getUserInstructions instr
    
    if Config.DEBUG then
        Utils.printHeaderMsg "USER INSTRUCTIONS"
        printfn $"%A{user_instructions}\n"
    
    user_instructions
    |> instructionsToTuples
    |> userListEmpty
    |> List.filter userIsRoot
    |> lastUserRoot


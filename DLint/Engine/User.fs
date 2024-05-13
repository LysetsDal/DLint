// ================================================
//             CHEKS ON USER DIRECTIVES
// ================================================
[<RequireQualifiedAccess>]
module DLint.Engine.User

open Absyn
open Types

module private UserScanInternals =
    open Rules.Misc.UserWarns
    
    /// <summary> Predicate filter for User instruction </summary>
    /// <param name="ins"> The instruction to check </param>
    let isUser (ins: instruction) =
        match ins with
        | User _ -> true
        | _ -> false
    
    
    /// <summary> Filter away non-user instructions </summary>
    /// <param name="lst"> The instruction list </param>
    let getUserInstructions lst =
        List.filter isUser lst

    
    /// <summary> Convert an instruction to a tuple: (line * (name * uid)) </summary>
    /// <param name="ins"></param>
    let instructionToTuple (ins: instruction) =
        match ins with
        | User (line, (Some name, Some uid)) -> (line, (name, uid))
        | User (line, (Some name, None)) -> (line, (name, -1))
        | User (line, (None, Some uid)) -> (line, ("", uid))
        | _ -> (-1, ("", -1))

    
    /// <summary> Convert the instruction list to a tuple list: (line * (name * uid)) list </summary>
    /// <param name="instrs"> The isntruction list </param>
    let instructionsToTuples (instrs: instruction list) =
        List.map instructionToTuple instrs
    
    
    /// <summary> Check if user name is root  </summary>
    /// <param name="user"> The user (line * (name * uid)) to check </param>
    let userNameIsRoot (user: int * (string * int)) =
        match user with
        | _, (name, _) ->
            match name.ToLower() with
            | "root" -> true
            | _ -> false
            
            
    /// <summary> Check if user uid is root  </summary>
    /// <param name="user"> The user (line * (name * uid) to check </param>
    let userIdIsRoot (user: int * (string * int)) =
        match user with
        | _, (_, id) ->
            match id with
            | 0  -> true
            | _ -> false
    
    
    /// <summary> Check if user(name || uid) is root </summary>
    /// <param name="user"> The user (line * (name * uid) to check </param>
    let userIsRoot (user: int * (string * int)) =
        (userNameIsRoot user || userIdIsRoot user) 
        
        
    /// <summary> Checks if no USER was provided in the dockerfile </summary>
    /// <param name="users"> The user list of: (line * (name * uid)) </param>
    let userListEmpty users =
        match List.length users with
        | 0 ->
            let warn = userWarn101
            Logger.log <| LogMiscWarnNoLine(warn)
            users
        | _ -> users

    
    /// <summary> If the last user was root, Log warning </summary>
    /// <param name="users"> List of (line * (name * uid)) elements </param>
    let lastUserRoot (users: (int * (string * int)) list) =
        match List.rev users with
        | (line, (name, uid)) :: _ ->
            let warn = userWarn100
            if userIsRoot (line, (name, uid)) then
                Logger.log <| LogMiscWarn (line, warn)
            else
                ()
        | _ -> ()         


// =======================================================
//                   Exposed Functions
// =======================================================
open UserScanInternals

/// <summary> Scans the syntax for USER privilege mode </summary>
/// <param name="instr"> The abstract syntax instruction list </param>
let scan (instr: instruction list) =       
    let user_instructions = getUserInstructions instr
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "USER INSTRUCTIONS")
        printfn $"%A{user_instructions}\n"
    
    user_instructions
    |> instructionsToTuples
    |> userListEmpty
    |> lastUserRoot

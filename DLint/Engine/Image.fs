// ================================================
//                LINTING BASE IMAGE
// ================================================
[<RequireQualifiedAccess>]
module DLint.Engine.Image

open Rules.Misc.ImageWarn
open Rules.Misc.EnvWarns
open Rules.MiscWarn
open Absyn
open Types

module private ImageHelpers =
    
    /// <summary> Filter predicate for BaseImage commands </summary>
    /// <param name="ins"> The instruction to filter on </param>
    let isImageInstruction (ins: instruction) =
        match ins with
        | BaseImage _ -> true
        | _ -> false
        
    
    /// <summary> Returns a list of all BaseImage instructions</summary>
    /// <param name="lst"> The list of instructions </param>
    let getImageInstructions (lst: instruction list) =
        List.filter isImageInstruction lst

    
    /// <summary> Check if a tag is latest </summary>
    /// <param name="tag"> The tag to check </param>
    let isLatest (tag: tag) =
        match tag with
        | Tag t ->
            match t.ToLower() with
            | "latest" -> true
            | _ -> false
        | _ -> failwith "Object passed not a tag!"

    
    /// <summary> Check if the image is using the 'latest' tag. Log warning if true. </summary>
    /// <param name="image"> </param>
    let imageTagIsLatest (image: instruction) =
        match image with
        | BaseImage(line, name, tag) ->
            if isLatest tag then
                let img_warn =
                    { imgWarn100 with Problem = $"FROM %s{name}:latest" }
                Logger.log <| LogMiscWarn (line, img_warn) 
        | _ -> ()



module private EnvVariableHelpers = 

    /// <summary> Predicate used to filter on Env types (line, envVar) </summary>
    /// <param name="ins"> The instruction to test predicate on </param>
    let isEnvInstruction (ins: instruction) =
        match ins with
        | Env _ -> true
        | _ -> false
        

    /// <summary> Get all Env instructions from instruction list </summary>
    /// <param name="lst"> List of instructions to filter </param>
    let getEnvInstructions (lst: instruction list) =
        List.filter isEnvInstruction lst
        
        
    /// <summary> Checks the key of the EnvVar for widely used sensitive variables (see EnvWarns.fs) </summary>
    /// <param name="e"> The Env (key-value pair) to test</param>
    let isSensitiveEnvVar (e: env) =
        match e with
        | EnvVar (key, _) ->
            match List.contains (key.ToLower()) bad_env with
            | true -> Some e
            | _ -> None


    /// <summary> Pattern match to extract key-value pair from an Env </summary>
    /// <param name="e"> The env to de-tuple </param>
    let getKeyVal (e:env) : string * string =
        match e with
        | EnvVar(key, value) -> key, value


    /// <summary> Constructs a new list of all sensitive Envs  </summary>
    /// <param name="lst"></param>
    let getSensitiveEnvs lst =
        let rec aux lst acc =
            match lst with
            | [] -> acc
            | (line, e) :: xs ->
                match isSensitiveEnvVar e with
                | Some e -> aux xs ((line, e) :: acc)
                | None -> aux xs acc
        aux lst []
        
        
    /// <summary> This function unwraps the Env instruction into EnvVars with line number.
    ///           This eases the logging of warnings </summary>
    /// <param name="lst"> List of all Env instructions </param>
    let getEnvVarsFromEnvs (lst: instruction list) =
        let rec aux lst acc =
            match lst with
            | [] -> acc
            | x :: xs ->
                match x with
                | Env (line, e) -> aux xs ((line, e)::acc)
                | _ -> aux xs acc
        aux lst []


    /// <summary> Log the sensitive env warnings to StdOut </summary>
    /// <param name="warn"> The line of the ENV docker instruction and the env key-value pair </param>
    let logEnvWarning (warn: int * env) =
        match warn with
        | line, env_var ->
            let key, value = getKeyVal env_var
            let env_warn = { envWarn100 with Problem = $"%s{key} = %s{value}" }
            Logger.log <| LogMiscWarn (line, env_warn)
        
        
    /// <summary> Scans through all ENV instructions, flagging commonly used constructs and logs them </summary>
    /// <param name="lst"> List of all instructions (AST) </param>
    let getSensitiveEnvVars (lst: instruction list) =
        let env_vars = getEnvVarsFromEnvs lst
        let sensitive_envs = getSensitiveEnvs env_vars
        List.iter logEnvWarning sensitive_envs
    
    
    
// =======================================================
//                   Exposed Function
// =======================================================
open ImageHelpers
open EnvVariableHelpers

/// <summary> The scan command of the Image module </summary>
/// <param name="instr"> A List of all the Dockerfile instructions (AST) </param>
let scan (instr: instruction list) =       
    let image_instructions = getImageInstructions instr
    let env_vars = getEnvInstructions instr
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "IMAGE INSTRUCTIONS")
        printfn $"%A{image_instructions}\n"
        
    if Config.DEBUG then
        Logger.log <| (LogHeader "ENVIRONMENT VARIABLES")
        printfn $"%A{env_vars}\n"
    
    image_instructions
    |> List.iter imageTagIsLatest
    
    env_vars
    |> getSensitiveEnvVars
    
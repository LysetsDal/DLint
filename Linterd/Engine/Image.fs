// ================================================
//                LINTING BASE IMAGE
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Image

open Rules.Misc.ImageWarn
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


// =======================================================
//                   Exposed Function
// =======================================================
open ImageHelpers

/// <summary> The scan command of the Image module </summary>
/// <param name="instr"> A List of all the Dockerfile instructions (AST) </param>
let scan (instr: instruction list) =       
    let image_instructions = getImageInstructions instr
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "IMAGE INSTRUCTIONS")
        printfn $"%A{image_instructions}\n"
    
    image_instructions
    |> List.iter imageTagIsLatest
    
// ================================================
//                Linting Base Image
// ================================================
[<RequireQualifiedAccess>]
module Linterd.Engine.Image

open Rules.Misc.ImageWarn
open Rules.MiscWarn
open Absyn
open Types

let isImageInstruction (ins: instruction) =
    match ins with
    | BaseImage _ -> true
    | _ -> false
    
    
let getImageInstructions (lst: instruction list) =
    List.filter isImageInstruction lst

let isLatest (tag: tag) =
    match tag with
    | Tag t ->
        match t.ToLower() with
        | "latest" -> true
        | _ -> false
    | _ -> failwith "Object passed not a tag!"

let imageTagIsLatest (image: instruction) =
    match image with
    | BaseImage(line, name, tag) ->
        if isLatest tag then
            let img_warn =
                { imgWarn100 with Problem = $"FROM %s{name}:latest" }
            Logger.log <| LogMiscWarn (line, img_warn) 
    | _ -> ()


let scan (instr: instruction list) =       
    let image_instructions = getImageInstructions instr
    
    if Config.DEBUG then
        Logger.log <| (LogHeader "IMAGE INSTRUCTIONS")
        printfn $"%A{image_instructions}\n"
    
    image_instructions
    // Only single stage builds supported currently
    |> List.head
    |> imageTagIsLatest 
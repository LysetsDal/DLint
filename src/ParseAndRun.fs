module ParseAndRun

open Program

let fromString = Parse.fromString
let fs = Parse.fromString

let fromFile = Parse.fromFile
let ff = Parse.fromFile

let interp = Interp.run

let run = Main
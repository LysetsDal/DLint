module Rules.Misc.ImageWarn

open Rules.MiscWarn

let imgWarn100 : MiscWarn = {
    ErrorCode = "IMGW100"
    Problem =  "latest"
    ErrorMsg = "Dont use the latests tag. Builds might break over time. Instead use a specific tag number."
}
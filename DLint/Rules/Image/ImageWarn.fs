module Rules.Misc.ImageWarn

open Rules.MiscWarn

let imgWarn100 : MiscWarn = {
    ErrorCode = "IMGW100"
    Problem =  "latest"
    ErrorMsg = "Don't use the latest tag. Builds might break, and additional packages installed. Instead use a specific tag number for the same version."
}
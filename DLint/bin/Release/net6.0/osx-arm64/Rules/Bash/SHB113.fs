module Rules.Bash.SHB113

open Rules.ShellWarn

let shb113 : BinWarn = {
    ErrorCode = "SHB113"
    Binary = "top"
    ErrorMsg = "Utilizing the top command within a Dockerfile at buildtime is nonsensical. It will only display processes running during image built time, and being interactive it can break the build entirely."
}
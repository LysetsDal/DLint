module Rules.Bash.SHB104

open Rules.ShellWarn

let shb104 : BinWarn = {
    ErrorCode = "SHB104"
    Binary = "kill"
    ErrorMsg = "Kill is used to terminate processes in linux. While you can run it inside a container, it is limited to only kill pids inside the container. Wrong use can also kill the container itself, and loose its state."
}
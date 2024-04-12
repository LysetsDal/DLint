module Rules.Bash.SHB113

open Rules.ShellWarn

let shb113 : BinWarn = {
    ErrorCode = "SHB113"
    Binary = "top"
    ErrorMsg = "Utilizing the top command within a Dockerfile (at buildtime) is nonsensical as its an interactive monitor for running processes on the system. Additionally, it will only display processes within the container's namespace, potentially diverging from the intended outcome."
}
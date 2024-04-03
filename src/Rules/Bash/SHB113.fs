module Linterd.Rules.Bash.SHB113

open Rules.ShellWarn

let shb113 : binWarn = {
    Code = "SHB113"
    Bin = "top"
    Msg = "Utilizing the top command within a Dockerfile (at buildtime) is non-
sensical as its an interactive monitor for running processes on the system. Ad-
ditionally, it will only display processes within the container's namespace, p-
otentially diverging from the intended outcome."
}
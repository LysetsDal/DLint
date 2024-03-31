module Linterd.Rules.Bash.SHB104

open Rules.ShellWarn

let shb104 : ShellWarn = {
    Code = "SHB104"
    Bin = "kill"
    Msg = "Kill is used to terminate processes in linux. While you can run it
inside a container, it is limited to only kill pids inside the container. Wrong
use can also kill the container itself, since its a process, and lose data. Co-
nsider remove kill from the command."
}
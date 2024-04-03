module Linterd.Rules.Bash.SHB112

open Rules.ShellWarn

let shb112 : binWarn = {
    Code = "SHB112"
    Bin = "ps"
    Msg = "Utilizing the ps command within a Dockerfile is impractical as it's
primarily designed to monitor running processes on the system. Additionally,
it will only display processes within the container's namespace, potentially
diverging from the intended outcome."
}
module Linterd.Rules.Bash.SHB109

open Rules.ShellWarn

let shb109 : binWarn = {
    Code = "SHB109"
    Bin = "mount"
    Msg = "Running mount inside a Docker container is impractical due to names-
pace isolation and the image-based filesystem docker uses. Containers should h-
ave their filesystemt mounted in the dockerfile through volumes, or from the r-
un command that starts the container."
}
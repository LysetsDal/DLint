module Rules.Bash.SHB109

open Rules.ShellWarn

let shb109 : BinWarn = {
    ErrorCode = "SHB109"
    Binary = "mount"
    ErrorMsg = "Running mount inside a Docker container is impractical due to namespace isolation and the image-based filesystem docker uses. Containers should have their filesystemt mounted in the dockerfile through volumes, or from the run command that starts the container."
}
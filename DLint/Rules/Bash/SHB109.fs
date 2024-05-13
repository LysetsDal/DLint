module Rules.Bash.SHB109

open Rules.ShellWarn

let shb109 : BinWarn = {
    ErrorCode = "SHB109"
    Binary = "mount"
    ErrorMsg = "Running a 'standard' mount in a RUN command should be avoided. Instead mount filesystems through volumes, or from the Docker specific 'RUN --mount=type...' command."
}
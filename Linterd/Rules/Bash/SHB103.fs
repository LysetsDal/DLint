module Rules.Bash.SHB103

open Rules.ShellWarn

let shb103 : binWarn = {
    ErrorCode = "SHB103"
    Binary = "free"
    ErrorMsg = "free displays the total amount of free and used physical and swap m-
emory in the system, as well as the buffers and caches used by the kernel. Run-
ning this inside a container doesnot make sense because of cgroups and namespa-
ce isolation. Remove free from the command."
}
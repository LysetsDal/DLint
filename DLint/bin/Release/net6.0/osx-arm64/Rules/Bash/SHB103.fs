module Rules.Bash.SHB103

open Rules.ShellWarn

let shb103 : BinWarn = {
    ErrorCode = "SHB103"
    Binary = "free"
    ErrorMsg = "free displays the total amount of free and used physical and swap memory in the system, as well as the buffers and caches used by the kernel. Running this inside the image build stage is discouraged."
}
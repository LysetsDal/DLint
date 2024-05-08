module Rules.Bash.SHB105

open Rules.ShellWarn

let shb105 : BinWarn = {
    ErrorCode = "SHB105"
    Binary = "ifconfig"
    ErrorMsg = "ifconfig is used to display information about the network interfaces. Running it inside the image build stage might not be meaningful. Use 'docker network ls' or 'docker network inspect' instead.
"
}
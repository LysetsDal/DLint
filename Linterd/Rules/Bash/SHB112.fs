module Rules.Bash.SHB112

open Rules.ShellWarn

let shb112 : BinWarn = {
    ErrorCode = "SHB112"
    Binary = "ps"
    ErrorMsg = "Utilizing the ps command within a Dockerfile is nonsensical as it will only display processes within the namespace at image build time."
}
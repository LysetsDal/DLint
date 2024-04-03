module Linterd.Rules.Bash.SHB202

open Rules.ShellWarn
let shb202 : aptWarn = {
    Code = "SHB201"
    Problem = ""
    Msg = "Use this flag to avoid the build requirering the user to input 'y'
which breaks the image build."
}
module Linterd.Rules.Bash.SHB203

open Rules.ShellWarn

let shb202 : aptWarn = {
    Code = "SHB201"
    Problem = ""
    Msg = "Missing both: --no-install-recommends && -y flags. Use these to avoid the
build requirering the user to input 'y' which breaks the image build (SHB114) and
the build requirering the user to input 'y' which breaks the image build (SHB115)."
}
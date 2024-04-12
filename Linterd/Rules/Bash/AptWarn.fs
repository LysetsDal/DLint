module Rules.Misc.AptWarn

open Rules.ShellWarn

let shb201 : AptWarn = {
    ErrorCode = "SHB201"
    Problem = ""
    ErrorMsg = "Missing: '-y'. Use this flag to avoid the build requiering the user to input 'y' which breaks the image build."
}

let shb202 : AptWarn = {
    ErrorCode = "SHBW202"
    Problem = ""
    ErrorMsg = "Missing: '--no-install-recommends'. Use this flag to avoid unnecessary packages being installed on your image."
}

let shb203 : AptWarn = {
    ErrorCode = "SHB203"
    Problem = ""
    ErrorMsg = "Missing both: '--no-install-recommends' && '-y' flags. Use these to avoid the build requirering the user to input 'y' which breaks the image build (SHB114) and the build requirering the user to input 'y' which breaks the image build (SHB115)."
}
module Rules.Bash.SHB110

open Rules.ShellWarn

let shb110 : binWarn = {
    Code = "SHB110"
    Bin = "nsenter"
    Msg = "While it is possible to use nsenter inside a container it is not re-
commended. Containerization best practices recommend against directly manipula-
ting namespaces from within a container, as it goes against the principle of e-
ncapsulation and isolation that containers provide."
}
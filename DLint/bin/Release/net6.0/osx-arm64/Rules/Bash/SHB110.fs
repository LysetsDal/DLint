module Rules.Bash.SHB110

open Rules.ShellWarn

let shb110 : BinWarn = {
    ErrorCode = "SHB110"
    Binary = "nsenter"
    ErrorMsg = "While it is possible to use nsenter inside a container it is not recommended. Containerization best practices recommend against directly manipulating namespaces from within a container, as it goes against the principle of encapsulation and isolation that containers provide."
}
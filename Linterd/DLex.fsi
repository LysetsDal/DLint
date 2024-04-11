module DLex

/// Rule Token
val Token: lexbuf: LexBuffer<char> -> token
/// Rule EndLineComment
val EndLineComment: lexbuf: LexBuffer<char> -> token
/// Rule Path
val Path: acc: obj -> lexbuf: LexBuffer<char> -> token
/// Rule Shell
val Shell: acc: obj -> lexbuf: LexBuffer<char> -> token
/// Rule String
val String: chars: obj -> lexbuf: LexBuffer<char> -> token

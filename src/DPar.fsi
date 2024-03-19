// Signature file for parser generated by fsyacc
module DPar
type token = 
  | EOF
  | COLON
  | DOT
  | LBRACK
  | RBRACK
  | EXPOSE
  | FSLASH
  | DASH
  | FROM
  | WORKDIR
  | USER
  | INT of (int)
  | RUNCMD of (string)
  | NAME of (string)
type tokenId = 
    | TOKEN_EOF
    | TOKEN_COLON
    | TOKEN_DOT
    | TOKEN_LBRACK
    | TOKEN_RBRACK
    | TOKEN_EXPOSE
    | TOKEN_FSLASH
    | TOKEN_DASH
    | TOKEN_FROM
    | TOKEN_WORKDIR
    | TOKEN_USER
    | TOKEN_INT
    | TOKEN_RUNCMD
    | TOKEN_NAME
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startMain
    | NONTERM_Main
    | NONTERM_File
    | NONTERM_BaseImg
    | NONTERM_Instrs
    | NONTERM_Instr
    | NONTERM_Runcmd
    | NONTERM_Expose
    | NONTERM_User
    | NONTERM_Version
    | NONTERM_MinorVersion
    | NONTERM_DashedName
    | NONTERM_Path
    | NONTERM_Dirs
/// This function maps tokens to integer indexes
val tagOfToken: token -> int

/// This function maps integer indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val Main : (FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> FSharp.Text.Lexing.LexBuffer<'cty> -> (Absyn.dockerfile) 

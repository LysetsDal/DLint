// ================================================
//            SET CONFIGS FOR LINTERD 
// ================================================

[<RequireQualifiedAccess>]
module Config

// LOGGING MODES
let DEBUG = true
let VERBOSE = true


// SHELLCHECK CONFIGS
let SHELLCHECK = "../shellcheck/shellcheck"
let SHELLCHECK_ARGS = "-s bash -f gcc"
let SHEBANG = "#!/bin/bash \n"
let OUTPUT_DIR = "./tmp/"


// VULNERABLE MOUNTS
let MNT_RULE_DIR = "./Rules/Mounts"

// VULNERABLE NETWORK 
let MISC_RULE_DIR = "./Rules/Misc"

// PROBLEMATIC BINARIES
let BASH_RULE_DIR = "./Rules/Bash"

let UNIX_MAX_PORT = 65535
let UNIX_MIN_PORT = 0
// ================================================
//            SET CONFIGS FOR DLINT 
// ================================================

[<RequireQualifiedAccess>]
module Config

// LOGGING LEVELS
let DEBUG = false
let VERBOSE = false
let mutable LOG_AS_CSV = false


// SHELLCHECK CONFIGS
let SHELLCHECK = "./Shellcheck/shellcheck"
let SHELLCHECK_ARGS = "-s bash -f gcc"
let SHEBANG_PREFIX = "#!/bin/bash \n"
let OUTPUT_DIR = "./tmp/"


// VULNERABLE MOUNTS
let MOUNT_RULE_DIR = "./Rules/Mounts"


// VULNERABLE NETWORK 
let NET_RULE_DIR = "./Rules/Net"
let UNIX_MAX_PORT = 65535
let UNIX_MIN_PORT = 0


// PROBLEMATIC BINARIES
let BASH_RULE_DIR = "./Rules/Bash"

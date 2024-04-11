// ================================================
//            SET CONFIGS FOR LINTERD 
// ================================================

[<RequireQualifiedAccess>]
module Config

// LOGGING LEVELS
let DEBUG = false
let VERBOSE = false
let LOG_MODE = true


// SHELLCHECK CONFIGS
let SHELLCHECK = "../shellcheck/shellcheck"
let SHELLCHECK_ARGS = "-s bash -f gcc"
let SHEBANG_PREFIX = "#!/bin/bash \n"
let OUTPUT_DIR = "./tmp/"


// VULNERABLE MOUNTS
let MOUNT_RULE_DIR = "./Rules/Mounts"


// VULNERABLE NETWORK 
let MISC_RULE_DIR = "./Rules/Misc"
let UNIX_MAX_PORT = 65535
let UNIX_MIN_PORT = 0


// PROBLEMATIC BINARIES
let BASH_RULE_DIR = "./Rules/Bash"

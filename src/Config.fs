// ================================================
//            SET CONFIGS FOR LINTERD 
// ================================================

[<RequireQualifiedAccess>]
module Config

// LOGGING MODES
let DEBUG = false
let VERBOSE = true


// SHELLCHECK CONFIGS
let SHELLCHECK = "../shellcheck/shellcheck"
let SHELLCHECK_ARGS = "-s bash -f gcc"
let SHEBANG = "#!/bin/bash \n"
let OUTPUT_DIR = "./tmp/"


// VULNERABLE MOUNTS
let MNT_RULE_DIR = "./Rules/Mounts"
let MISC_RULE_DIR = "./Rules/Misc"
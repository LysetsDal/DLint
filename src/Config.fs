module Config

// LOGGING MODES
let DEBUG = false
let VERBOSE = true

// SHELLCHECK CONFIGS
let SHELLCHECK_PATH = "../shellcheck/shellcheck"
let SHELLCHECK_ARGS = "-s bash -f gcc"
let SHEBANG = "#!/bin/bash \n"
let SHELL_CMD_DELIMS = [|"&&"; ";"; "|"; "<<"; ">>"|]

// OUTPUT
let OUTPUT_DIR = "./out/"
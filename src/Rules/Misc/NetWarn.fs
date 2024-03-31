module Rules.Misc.NetWarn100

open Rules.MiscWarning

let netWarn100 : MiscWarn = {
    Code = "Info:"
    Problem =  "--network=host"
    Msg = "RUN --network allows control over which networking environment the
command is run in. With this setting the container is run in the host's netw-
ork environment. The use of --network=host should be carefully considered, as
it can leak the host machines network interface to the container."
}

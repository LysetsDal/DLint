FROM ubuntu:18.9-beta
# I am a comment 
USER root 1001

WORKDIR /etc/tmp/

RUN --mount=type=bind, target=/var/run/docker.sock 2&1>/dev/null

RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \ 
    && --mount=type=cache,target=/var/lib/apt,sharing=locked \ 
    && apt update && apt-get --no-install-recommends install -y gcc

EXPOSE 80:8080
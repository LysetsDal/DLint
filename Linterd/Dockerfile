# Base image warning
FROM ubuntu:latest

USER asly 
WORKDIR /tmp/var

## Env vars
ENV DB_KEY="Secret_key" 
ENV CONNECTION="127.0.0.0"

ENV LANG=en_US.utf8
ENV TZ=America/New_York
ENV ZIGPATH=/opt/zig

## Shellcheck 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone && \
    rm -rf /var/lib/apt/lists/* && \
    localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8

## Volume mounts
VOLUME /data/acubiz.db
VOLUME /dev/sda1
VOLUME /var/docker.sock

## User test
USER root

## apt-install checks:
RUN sudo apt-get update && \
    apt-get install -y --no-install-recommends libxzma && \
    apt-get install -y nginx && \
    apt-get install --no-install-recommends apache && \
    apt-get install exploit_db 

## Run Mounts
RUN --mount=type=cache,target=/var/cache/apt,sharing=locked && \
  --mount=type=cache,target=/proc/sys/kernel/modprobe && \
  apt update && apt-get install --no-install-recommends -y gcc

## Binaries 
RUN mkdir -p /app/myapp && \
    cd ./app || exit && \
    ssh -P 22 dummy_user@172.132.0.23 && \
    shutdown -h now 
    
## Network 
RUN --network=host pip install vulnerable_lib 

## Port scan
EXPOSE 222
EXPOSE 80:8080
EXPOSE 21, 22, 443

EXPOSE 0
EXPOSE 69420
EXPOSE 80:69420
EXPOSE 22, 80, 65536
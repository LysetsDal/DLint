FROM ubuntu:18.9-beta
# I am a comment 
USER toor 

USER 1001

WORKDIR /etc/tmp/

RUN --mount=type=bind, target=/var/run/docker.sock 2&1>/dev/null

RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \ 
    && --mount=type=cache,target=/var/lib/apt,sharing=locked \ 
    && apt update && apt-get --no-install-recommends install -y gcc

COPY /www/test.two ./test/

COPY . .

EXPOSE 22, 80, 443

EXPOSE 80:8080

FROM ubuntu:18.9-beta
# I am a comment 
USER toor 

USER 1001

ENV SECRET_KEY="very secret key"

WORKDIR /etc/tmp/

RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    && sudo apt update && apt-get --no-install-recommends install -y gcc \

RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone && \
    apt-get update && \
    apt-get install -y locales && \
    rm -rf /var/lib/apt/lists/* && \
    localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8 && \
    apt-get update && \
    apt-get install -y vim

COPY /www/test.two ./test/

COPY . .

EXPOSE 22, 80, 443

EXPOSE 80:8080
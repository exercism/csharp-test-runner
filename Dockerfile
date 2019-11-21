FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-alpine3.10
WORKDIR /opt/test-runner/bin

RUN apk add --no-cache \
    bash \
    wget \
    curl \
    icu-libs \
    openssl

RUN wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && \
    chmod +x /tmp/dotnet-install.sh && \
    /tmp/dotnet-install.sh -Channel 2.1 -Runtime dotnet -InstallDir /usr/share/dotnet && \
    /tmp/dotnet-install.sh -Channel 2.2 -Runtime dotnet -InstallDir /usr/share/dotnet

COPY run.sh .
COPY run.ps1 .

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-alpine3.10 AS build-env
WORKDIR /app

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

COPY run.sh /opt/test-runner/bin/

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-alpine3.10
WORKDIR /opt/test-runner
COPY --from=build-env /opt/test-runner/ .
ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

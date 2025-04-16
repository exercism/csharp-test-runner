FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

WORKDIR /tmp

# Pre-install packages for offline usage
RUN dotnet new console && \
    dotnet add package Exercism.Tests --version 0.1.0-beta1 && \
    dotnet add package Exercism.Tests.xunit.v3 --version 0.1.0-beta1 && \
    dotnet add package FakeItEasy --version 6.2.1 && \
    dotnet add package FsCheck --version 2.14.4 && \
    dotnet add package FsCheck --version 3.1.0 && \
    dotnet add package FsCheck.Xunit --version 2.14.4 && \
    dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0 && \
    dotnet add package Microsoft.Reactive.Testing --version 5.0.0 && \
    dotnet add package Sprache --version 2.3.1 && \
    dotnet add package xunit --version 2.8.1 && \
    dotnet add package xunit.v3 --version 1.1.0 && \
    dotnet add package xunit.runner.visualstudio --version 3.0.1 && \
    dotnet add package BenchmarkDotNet --version 0.14.0 && \
    dotnet add package Microsoft.Extensions.TimeProvider.Testing --version 9.2.0

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -a $TARGETARCH

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -a $TARGETARCH -c Release -o /opt/test-runner --no-restore

# Build runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV DOTNET_ROLL_FORWARD=Major
ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

# RUN apk add --no-cache icu-libs icu-data-full tzdata
WORKDIR /opt/test-runner

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/
COPY --from=build /root/.nuget/packages/ /root/.nuget/packages/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

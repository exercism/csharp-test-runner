FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.20 AS build
ARG TARGETARCH

WORKDIR /tmp

# Pre-install packages for offline usage
RUN dotnet new console && \
    dotnet add package Exercism.Tests --version 0.1.0-beta1 && \
    dotnet add package FakeItEasy --version 6.2.1 && \
    dotnet add package FsCheck --version 2.14.4 && \
    dotnet add package FsCheck.Xunit --version 2.14.4 && \
    dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0 && \
    dotnet add package Microsoft.Reactive.Testing --version 5.0.0 && \
    dotnet add package Sprache --version 2.3.1 && \
    dotnet add package xunit --version 2.8.1 && \
    dotnet add package xunit.runner.visualstudio --version 3.0.1

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -a $TARGETARCH

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -a $TARGETARCH -c Release -o /opt/test-runner --no-restore

# Build runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.21 AS runtime

# Enable globalization as some exercises use it
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs icu-data-full tzdata
WORKDIR /opt/test-runner

# Enable rolling forward the .NET SDK used to be backwards-compatible
ENV DOTNET_ROLL_FORWARD=Major

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/
COPY --from=build /root/.nuget/packages/ /root/.nuget/packages/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

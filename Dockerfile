FROM mcr.microsoft.com/dotnet/sdk:9.0.200-alpine3.21-amd64 AS build

WORKDIR /tmp

# Pre-install packages for offline usage
RUN dotnet new console && \
    dotnet add package Microsoft.NET.Test.Sdk -v 17.12.0 && \
    dotnet add package xunit -v 2.8.1 && \
    dotnet add package xunit.runner.visualstudio -v 3.0.1 && \
    dotnet add package Exercism.Tests -v 0.1.0-alpha

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:9.0.200-alpine3.21-amd64 AS runtime
WORKDIR /opt/test-runner

# Enable rolling forward the .NET SDK used to be backwards-compatible
ENV DOTNET_ROLL_FORWARD Major

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/
COPY --from=build /root/.nuget/packages/ /root/.nuget/packages/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

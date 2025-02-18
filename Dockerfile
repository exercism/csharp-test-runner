FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.20-amd64 AS build
WORKDIR /app

# Pre-install packages for offline usage
RUN dotnet add package Microsoft.NET.Test.Sdk -v 16.8.3 && \
    dotnet add package xunit -v 2.4.1 && \
    dotnet add package xunit.runner.visualstudio -v 2.4.3 && \
    dotnet add package Exercism.Tests -v 0.1.0-alpha

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner --no-restore --self-contained true

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.20-amd64 AS runtime
WORKDIR /opt/test-runner

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/
COPY --from=build /root/.nuget/packages/ /root/.nuget/packages/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-alpine3.12 AS build
WORKDIR /app

# Download exercism tooling webserver
RUN wget -P /usr/local/bin https://github.com/exercism/tooling-webserver/releases/download/0.10.0/tooling_webserver && \
    chmod +x /usr/local/bin/tooling_webserver

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner --no-restore

# Pre-install packages for offline usage
RUN dotnet add package Microsoft.NET.Test.Sdk -v 16.5.0 && \
    dotnet add package xunit -v 2.4.1 && \
    dotnet add package xunit.runner.visualstudio -v 2.4.1

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-alpine3.12 AS runtime
WORKDIR /opt/test-runner

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/
COPY --from=build /root/.nuget/packages/ /root/.nuget/packages/

COPY run.sh /opt/test-runner/bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

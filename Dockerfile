FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app

COPY run.sh /opt/test-runner/bin/

# Download exercism tooling webserver
RUN wget -P /usr/local/bin https://github.com/exercism/local-tooling-webserver/releases/latest/download/exercism_local_tooling_webserver && \
    chmod +x /usr/local/bin/exercism_local_tooling_webserver

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS runtime
WORKDIR /opt/test-runner
COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

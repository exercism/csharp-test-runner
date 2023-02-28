FROM mcr.microsoft.com/dotnet/sdk:7.0.201-alpine3.16-amd64 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -r linux-musl-x64 -c Release -o /opt/test-runner --no-restore --self-contained true

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0.3-alpine3.16-amd64 AS runtime

# Enable globalization as some exercises use it
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs icu-data-full tzdata

WORKDIR /opt/test-runner

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

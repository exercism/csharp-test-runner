FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.20 AS build
ARG TARGETARCH
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Exercism.TestRunner.CSharp/Exercism.TestRunner.CSharp.csproj ./
RUN dotnet restore -a $TARGETARCH

# Copy everything else and build
COPY src/Exercism.TestRunner.CSharp/ ./
RUN dotnet publish -a $TARGETARCH -c Release -o /opt/test-runner --no-restore --self-contained true

# Build runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:9.0-alpine3.20 AS runtime

# Enable globalization as some exercises use it
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs icu-data-full tzdata

WORKDIR /opt/test-runner

COPY --from=build /opt/test-runner/ .
COPY --from=build /usr/local/bin/ /usr/local/bin/

COPY run.sh bin/

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

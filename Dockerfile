FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine
WORKDIR /opt/test-runner/bin

COPY run.sh .

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

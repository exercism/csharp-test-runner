FROM mcr.microsoft.com/dotnet/core/sdk:2.2.402-alpine3.9
WORKDIR /opt/test-runner/bin

# Install PowerShell
RUN wget -q https://github.com/PowerShell/PowerShell/releases/download/v6.2.3/powershell-6.2.3-linux-alpine-x64.tar.gz -O /tmp/linux.tar.gz && \
    mkdir -p /opt/microsoft/powershell/6 && \
    tar zxf /tmp/linux.tar.gz -C /opt/microsoft/powershell/6 -v && \
    ln -s /opt/microsoft/powershell/6/pwsh /usr/bin/pwsh && \
    chmod a+x,o-w /opt/microsoft/powershell/6/pwsh && \
    rm -f /tmp/linux.tar.gz

COPY run.sh .
COPY run.ps1 .

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

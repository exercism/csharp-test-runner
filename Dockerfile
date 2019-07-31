FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine
WORKDIR /opt/test-runner/bin

# Create test runner script
RUN printf "#!/bin/sh\nexport DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true\nfind \$2 -type f -name *Test.cs -exec sed -i 's/Skip = \"Remove to run test\"//g' {} \;\ndotnet test \$2" > run.sh && \
    chmod +x run.sh

ENTRYPOINT ["sh", "/opt/test-runner/bin/run.sh"]

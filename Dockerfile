# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build

WORKDIR /src

# Copy project files first for better layer caching
COPY *.csproj ./
COPY packages.config ./

# Copy everything else
COPY . .

# Restore NuGet packages
RUN nuget restore Demodeck.Legacy.Api.csproj -PackagesDirectory packages

# Build and publish
RUN msbuild Demodeck.Legacy.Api.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=C:\src\published

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022 AS runtime

WORKDIR /inetpub/wwwroot

# Download LogMonitor and ServiceMonitor for capturing logs to stdout
ADD https://github.com/microsoft/windows-container-tools/releases/download/v1.2/LogMonitor.exe C:\\LogMonitor.exe
ADD https://github.com/microsoft/IIS.ServiceMonitor/releases/download/v2.0.1.10/ServiceMonitor.exe C:\\ServiceMonitor.exe

# Copy built files from build stage
COPY --from=build "C:/src/published/_PublishedWebsites/Demodeck.Legacy.Api" "C:\\inetpub\\wwwroot"

# Configure LogMonitor - LogMonitor v1.2 looks for config at C:\LogMonitor\LogMonitorConfig.json
RUN powershell -Command "New-Item -ItemType Directory -Path C:\LogMonitor -Force; New-Item -ItemType Directory -Path C:\inetpub\logs\LogFiles -Force"
COPY LogMonitorConfig.json C:\\LogMonitor\\LogMonitorConfig.json

# Disable DiagTrack service to prevent crashes in container
RUN powershell -Command "Stop-Service -Name DiagTrack -ErrorAction SilentlyContinue; Set-Service -Name DiagTrack -StartupType Disabled -ErrorAction SilentlyContinue"

# Set environment variables (can be overridden at runtime via Kubernetes ConfigMap/Secrets)
ENV AppSettings__ServiceName="Demodeck.Legacy.Api"
ENV AppSettings__Version="1.0.0"
ENV AppSettings__Environment="Production"

# Expose HTTP port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD powershell -command "try { $response = Invoke-WebRequest -Uri http://localhost/api/health -UseBasicParsing -TimeoutSec 5; if ($response.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }"

# Use LogMonitor with ServiceMonitor to capture all logs
SHELL ["cmd", "/S", "/C"]
ENTRYPOINT ["C:\\LogMonitor.exe", "C:\\ServiceMonitor.exe", "w3svc"]

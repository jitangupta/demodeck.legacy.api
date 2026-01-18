# Windows Server Core with .NET Framework 4.8 and ASP.NET
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# Set working directory
WORKDIR /inetpub/wwwroot

# Copy application files from bin folder (after VS build)
COPY ./bin/ ./bin/
COPY ./Web.config ./
COPY ./Global.asax ./

# Set environment variables (can be overridden at runtime via -e or Kubernetes env)
ENV AppSettings__ServiceName="Demodeck.Legacy.Api"
ENV AppSettings__Version="1.0.0"
ENV AppSettings__Environment="Production"

# Expose port 80 (IIS default)
EXPOSE 80

# Health check - Windows PowerShell syntax
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD powershell -command "try { $response = Invoke-WebRequest -Uri http://localhost/api/health -UseBasicParsing -TimeoutSec 5; if ($response.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }"

# IIS runs automatically as ENTRYPOINT in the base image

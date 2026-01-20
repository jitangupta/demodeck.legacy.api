# Demodeck.Legacy.Api

ASP.NET Framework 4.8 Web API for Kubernetes deployment.

## Docker Image

```
demodeckacr.azurecr.io/legacy-api:v1.0.1
```

## Build

```bash
docker build -t demodeckacr.azurecr.io/legacy-api:v1.0.1 .
```

## Run Locally

```bash
docker run -d -p 8080:80 -e AppSettings__Environment=Development demodeckacr.azurecr.io/legacy-api:v1.0.1
```

## Push to ACR

```bash
az acr login --name demodeckacr
docker push demodeckacr.azurecr.io/legacy-api:v1.0.1
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `AppSettings__ServiceName` | Demodeck.Legacy.Api | Service name |
| `AppSettings__Version` | 1.0.0 | Application version |
| `AppSettings__Environment` | Production | Environment name |

## Endpoints

- `GET /api/health` - Health check
- `GET /api/greet` - Greeting endpoint
- `GET /api/greet/{name}` - Personalized greeting

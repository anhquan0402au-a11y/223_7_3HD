# Order API DevOps Pipeline (SIT223/SIT753 - 7.3HD)

A C# ASP.NET Core Web API (in-memory Order Management) wired into a **7-stage
Jenkins pipeline**: Build -> Test -> Code Quality -> Security -> Deploy ->
Release -> Monitoring. Everything runs locally in Docker Desktop.

> Full step-by-step instructions are in **GUIDE.md**. Read that first.

## Repository layout
```
src/OrderApi/              ASP.NET Core Web API (the app)
tests/OrderApi.Tests/      xUnit unit + integration tests
Dockerfile                 Multi-stage build of the app image
docker-compose.staging.yml Deploy stage  -> staging container (port 8081)
docker-compose.prod.yml    Release stage -> production container (port 8080)
Jenkinsfile                The 7-stage declarative pipeline
jenkins/                   Custom Jenkins image (Docker CLI + .NET SDK + Trivy)
sonarqube-docker-compose.yml   Code Quality server (port 9000)
monitoring/                Prometheus + Grafana + alert rules (ports 9090 / 3000)
GUIDE.md                   Complete build-and-run instructions
```

## Port map (localhost)
| Service            | URL                     |
|--------------------|-------------------------|
| Jenkins            | http://localhost:8085   |
| SonarQube          | http://localhost:9000   |
| App - staging      | http://localhost:8081   |
| App - production   | http://localhost:8080   |
| Prometheus         | http://localhost:9090   |
| Grafana            | http://localhost:3000   |

## Run the app by itself (quick check)
```bash
dotnet run --project src/OrderApi        # then open http://localhost:5xxx/swagger
dotnet test tests/OrderApi.Tests         # run the tests
```

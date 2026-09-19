# SIT223/SIT753 - 7.3HD DevOps Pipeline: Complete Step-by-Step Guide

This guide takes you from an empty machine to all **7 Jenkins stages green**, a
demo video, and a completed report. Follow it in order. Do NOT skip the
verification (`Check:`) lines - they are how you avoid wasting your one
resubmission.

> **You have only 2 submissions.** Get every stage green and screenshot BEFORE
> your first submit. (VN: Bạn chỉ có 2 lần nộp. Phải chụp màn hình đủ 7 stage
> xanh TRƯỚC khi nộp lần đầu.)

---
## Port map (memorise these)
| Service          | URL                    | Notes                          |
|------------------|------------------------|--------------------------------|
| Jenkins          | http://localhost:8085  | the CI server                  |
| SonarCloud       | https://sonarcloud.io  | code quality (cloud, no local port)|
| App - staging    | http://localhost:8081  | Deploy stage output            |
| App - production | http://localhost:8080  | Release stage output           |
| Prometheus       | http://localhost:9090  | metrics + alerts               |
| Grafana          | http://localhost:3000  | dashboard (login admin/admin)  |

Inside the Jenkins container, the host is reached as `host.docker.internal`.

---
## Apple Silicon (MacBook M-chip) notes
Good news: the whole stack runs **natively on arm64**, no config changes needed.
- Jenkins, Prometheus, Grafana, Trivy, .NET 8 images: all arm64-native.
- **Code Quality uses SonarCloud** (cloud, https://sonarcloud.io) - nothing to
  run locally, so no SonarQube container and no arm64/RAM concerns at all.
  (VN: Dùng SonarCloud trên mây, không chạy SonarQube local nên khỏi lo chip M.)
- When you install the **.NET SDK 8** for local testing, pick the **macOS
  Arm64** installer (NOT the x64/Intel one) or use `brew install --cask
  dotnet-sdk`. (VN: Tải đúng bản Arm64, đừng tải bản Intel.)
- Optional safety net: Docker Desktop -> Settings -> General -> enable **"Use
  Rosetta for x86/amd64 emulation"** (not required here, but harmless).
- The app image you push to Docker Hub will be arm64. That is fine - the whole
  demo runs on your Mac, and the Dockerfile rebuilds on any architecture.
- Your Terminal is zsh/bash, so every command in this guide works as-is (no
  PowerShell differences to worry about).

---
## PHASE 0 - Install the tools

1. **Docker Desktop** - install and start it. Confirm it is running:
   ```bash
   docker --version
   docker compose version
   ```
2. **Git** - `git --version`.
3. **.NET SDK 8** (only needed to test the app on your laptop, not for the
   pipeline): https://dotnet.microsoft.com/download/dotnet/8.0 then `dotnet --version`.
4. In Docker Desktop -> Settings -> Resources, give it at least **4 GB RAM**
   (Jenkins + the app + monitoring run together). (VN: Để RAM >= 4GB cho mượt.)

---
## PHASE 1 - Get the code building and pushed to GitHub

1. Copy this whole `order-api-devops/` folder to where you keep projects.
2. Test it locally first (proves the app is healthy before Jenkins touches it):
   ```bash
   cd order-api-devops
   dotnet test tests/OrderApi.Tests/OrderApi.Tests.csproj
   ```
   **Check:** all tests pass (6 unit + 5 integration). If a NuGet restore is
   slow the first time, that is normal.
3. (Optional, for Visual Studio only) create a solution file:
   ```bash
   dotnet new sln -n OrderApi
   dotnet sln add src/OrderApi/OrderApi.csproj tests/OrderApi.Tests/OrderApi.Tests.csproj
   ```
   The pipeline does **not** need the .sln - it targets the .csproj files directly.
4. Create a **GitHub repo** (private is fine) and push:
   ```bash
   git init
   git add .
   git commit -m "Initial commit: Order API + 7-stage Jenkins pipeline"
   git branch -M main
   git remote add origin https://github.com/anhquan0402au-a11y/order-api-devops.git
   git push -u origin main
   ```
5. **CRITICAL - grant access.** GitHub repo -> Settings -> Collaborators ->
   add **your Marker** and the **Unit Chair** (usernames/emails from the unit
   site). This is explicitly required; forgetting it wastes your feedback
   round. (VN: Nhớ add Marker + Unit Chair làm collaborator, nếu quên là mất
   lượt feedback duy nhất.)

---
## PHASE 2 - Start Jenkins

1. Build and start the custom Jenkins (Jenkins + Docker CLI + .NET SDK + Trivy):
   ```bash
   cd jenkins
   docker compose -f docker-compose.jenkins.yml up -d --build
   ```
   First build takes a few minutes (it downloads the .NET SDK and Trivy).
2. Get the first-run admin password:
   ```bash
   docker exec jenkins cat /var/jenkins_home/secrets/initialAdminPassword
   ```
3. Open **http://localhost:8085**, paste the password, choose **Install
   suggested plugins**, and create your admin user.
4. Install two extra plugins: **Manage Jenkins -> Plugins -> Available**, search
   and install **"Docker Pipeline"** and **"JUnit"** (JUnit is usually already
   there). Restart Jenkins if asked.
5. **Check** the tools exist inside the container:
   ```bash
   docker exec jenkins dotnet --version
   docker exec jenkins docker --version
   docker exec jenkins trivy --version
   ```
   All three must print a version.

---
## PHASE 3 - Set up SonarCloud (Code Quality - cloud)

You are using **SonarCloud**, not a local SonarQube. Skip the local container.

1. Go to https://sonarcloud.io and log in with your **GitHub** account.
2. **+ -> Analyze new project -> import** your `223_7_3HD` repo (create the
   organization `anhquan0402au-a11y` if asked). Your project key becomes
   `anhquan0402au-a11y_223_7_3HD`.
3. **CRITICAL - switch to CI analysis.** In the project:
   **Administration -> Analysis Method -> turn OFF "Automatic Analysis"** and
   choose **"CI-based analysis / Other CI"**. If you leave Automatic Analysis on,
   the Jenkins scan fails with "you are running CI analysis while Automatic
   Analysis is enabled". (VN: BẮT BUỘC tắt Automatic Analysis, nếu không Jenkins
   scan sẽ báo lỗi.)
4. Generate a token: avatar -> **My Account -> Security -> Generate Token**.
   Copy it (you paste it into Jenkins in Phase 4).
5. Confirm your **organization key** and **project key** on the project's
   Information page - they must match the `SONAR_ORG` and `SONAR_PROJECT_KEY`
   values at the top of the Jenkinsfile. Edit the Jenkinsfile if yours differ.

> Note: SonarCloud is free for **public** repositories. If `223_7_3HD` is
> private, either make it public or use SonarCloud's free-tier allowance.

## PHASE 4 - Credentials, Docker Hub, and the shared network

1. **Docker Hub** - create a free account at hub.docker.com and a **Access
   Token** (Account Settings -> Security -> New Access Token). You will use your
   username + that token in Jenkins.
2. In Jenkins: **Manage Jenkins -> Credentials -> System -> Global -> Add
   Credentials**. Add these **two**:
   - Kind **Username with password** - ID exactly `dockerhub-creds` -
     username = your Docker Hub username, password = the Docker Hub token.
   - Kind **Secret text** - ID exactly `sonarcloud-token` - secret = the
     SonarCloud token from Phase 3.
   > The IDs must match the Jenkinsfile exactly, or the build fails.
3. Create the shared network once (Prometheus uses it to reach production):
   ```bash
   docker network create devops-net
   ```

---
## PHASE 5 - Create the pipeline job

1. Jenkins home -> **New Item** -> name `orderapi-pipeline` -> **Pipeline** -> OK.
2. Scroll to **Pipeline** section -> Definition = **Pipeline script from SCM**.
   - SCM = **Git**
   - Repository URL = your GitHub URL
   - Credentials = add your GitHub username + a GitHub Personal Access Token
     (Settings -> Developer settings -> Tokens on GitHub) if the repo is private
   - Branch = `*/main`
   - Script Path = `Jenkinsfile`
3. **Save**.
4. (Top HD - automatic trigger) In the job config tick **"GitHub hook trigger"**
   or **"Poll SCM"** with schedule `H/2 * * * *` so a `git push` starts a build
   automatically. Mention this in your report/video as your "trigger".

---
## PHASE 6 - Run it and verify each stage

Click **Build Now**. Watch **Stage View** / **Console Output**. Here is what each
stage does and how to prove it worked (screenshot each one for the report).

### Stage 1 - Build
Runs `dotnet build`, then `docker build` to create image `orderapi:<build#>` and
`orderapi:latest`.
**Check:** console shows `docker images orderapi` listing your tagged image.

### Stage 2 - Test
Runs `dotnet test` with a JUnit logger + coverage collection. The `post` block
publishes results so Jenkins draws a **Test Result Trend** graph.
**Check:** the build page shows "Test Result" with 11 tests passed. A failing
test turns the stage red and stops the pipeline (that is the pass/fail gate).

### Stage 3 - Code Quality (SonarCloud)
`dotnet-sonarscanner begin` (with `/o:` organization + `/k:` project key) ->
`dotnet build` -> `end`, with `sonar.qualitygate.wait=true` so the pipeline
**waits for and enforces** the SonarCloud Quality Gate.
**Check:** https://sonarcloud.io -> project `223_7_3HD` shows bugs, code smells,
duplications and a Quality Gate status. Screenshot the project overview.
(VN: Đây là "gated check" - Quality Gate fail thì pipeline fail.)

### Stage 4 - Security (Trivy)
Scans dependencies (`trivy fs`) and the image (`trivy image`) for HIGH/CRITICAL
vulnerabilities. Reports are printed to the console AND archived as build
artefacts.
**Check:** build page -> "Build Artifacts" -> `trivy-fs-report.txt`,
`trivy-image-report.txt`. Read them; for the report, pick one finding and write:
*what it is, its severity, and how you addressed it* (e.g. "no fix available -
documented as accepted", or "updated base image to patch it"). This explanation
is required for marks.

### Stage 5 - Deploy (staging)
`docker compose -f docker-compose.staging.yml up -d` starts `orderapi-staging`
on port 8081, then curls `/health` (build fails if unhealthy).
**Check:** http://localhost:8081/swagger works; `/health` returns
`{"status":"healthy"}`.

### Stage 6 - Release (production)
Logs in to Docker Hub, tags + pushes the image (versioned + `latest`), then runs
`docker-compose.prod.yml` to start `orderapi-prod` on port 8080 on `devops-net`.
**Check:** your image appears on hub.docker.com; http://localhost:8080/health
returns healthy. (VN: Đây là "versioned, automated release".)

### Stage 7 - Monitoring
Starts Prometheus + Grafana on `devops-net`, curls their health endpoints.
**Check:**
- http://localhost:9090/targets -> job `orderapi` is **UP**.
- http://localhost:9090/alerts -> your `OrderApiDown` / `HighHttp5xxRate` rules
  are listed.
- http://localhost:3000 (admin/admin) -> "Order API Monitoring" dashboard shows
  panels with live data.

---
## Incident simulation (do this in the video - it earns Top HD)
1. Generate traffic so the graphs move:
   ```bash
   for i in $(seq 1 50); do curl -s http://localhost:8080/api/orders > /dev/null; done
   ```
2. Kill production to trigger the alert:
   ```bash
   docker stop orderapi-prod
   ```
   Wait ~20s -> http://localhost:9090/alerts shows **OrderApiDown = FIRING**.
   Show it in the video, then bring it back: `docker start orderapi-prod`.
(VN: Bước "incident simulation" này là điểm cộng lớn cho Top HD.)

---
## TROUBLESHOOTING (the things that actually break)
- **`docker: permission denied` in a stage** - the Jenkins compose already runs
  as `user: root`, which fixes this. If you changed that, revert it.
- **`host.docker.internal` not found** - you are on Linux (not Docker Desktop).
  Fix: add `--add-host=host.docker.internal:host-gateway` support by adding
  `extra_hosts: ["host.docker.internal:host-gateway"]` to the jenkins service,
  or replace it with your host IP.
- **Code Quality stage fails with "Automatic Analysis is enabled"** - turn OFF
  Automatic Analysis in SonarCloud (Phase 3, step 3).
- **Code Quality stage fails on auth / project not found** - check the
  `sonarcloud-token` credential and that `SONAR_ORG` / `SONAR_PROJECT_KEY` in the
  Jenkinsfile match your SonarCloud project exactly.
- **Release stage fails at push** - check `dockerhub-creds` (username + access
  token, not your web password).
- **Prometheus target DOWN** - `orderapi-prod` must be on `devops-net`. Re-run
  `docker network create devops-net`, then re-run the Release + Monitoring stages.
- **Port already in use** - something else uses 8080/3000; stop it or change
  the host port in the relevant compose file.

---
## DEMO VIDEO SCRIPT (<= 10 minutes)
1. (0:30) Show the GitHub repo + the Jenkinsfile; confirm Marker/Unit Chair access.
2. (1:00) Show how to clone and that the pipeline is "Pipeline script from SCM".
3. (5:00) Click Build Now; narrate each of the 7 stages as they go green,
   showing: test trend, SonarCloud dashboard, a Trivy finding, staging /health,
   Docker Hub push, production /health.
4. (2:00) Monitoring: Prometheus target UP, Grafana dashboard, then the incident
   simulation (stop prod -> alert fires -> restart).
5. (0:30) Show the final deployed app in Swagger creating an order.
Keep it tight; the rubric rewards "fluent narration".

---
## REPORT (fill the Deakin template) - what goes in each section
- **Project description + technologies:** ASP.NET Core 8 Web API (Order
  Management, CRUD + validation + API-key auth), xUnit, Docker, Jenkins,
  SonarCloud, Trivy, Docker Hub, Prometheus, Grafana.
- **Number of stages implemented:** 7.
- **Repo link:** your GitHub URL (with Marker + Unit Chair access).
- **Video link:** unlisted YouTube / OneDrive / Deakin cloud.
- **Jenkins screenshot:** the Stage View with all 7 stages green.
- **Per-stage description (with tools):** one short paragraph per stage - copy
  the "what it does" lines above and add your screenshot.
- **Security explanation:** the what/severity/how-addressed for a Trivy finding.

---
## Self-check against the rubric (aim: Top HD)
- [ ] All 7 stages implemented and green
- [ ] Build produces a versioned, tagged image (BUILD_NUMBER)
- [ ] Test: unit + integration, pass/fail gates the pipeline, trend published
- [ ] Code Quality: Quality Gate enforced (`qualitygate.wait=true`), metrics explained
- [ ] Security: findings interpreted + addressed/justified in the report
- [ ] Deploy: automated to staging + smoke-tested
- [ ] Release: versioned image pushed to Docker Hub + prod brought up automatically
- [ ] Monitoring: live dashboard + alert rule that fires (incident simulation)
- [ ] Automatic trigger on push (poll SCM / webhook)
- [ ] Marker + Unit Chair have repo access

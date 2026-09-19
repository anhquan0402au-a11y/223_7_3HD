pipeline {
    agent any

    environment {
        IMAGE_NAME        = 'orderapi'
        IMAGE_TAG         = "${env.BUILD_NUMBER}"
        DOCKER_NETWORK    = 'devops-net'

        // ----- SonarCloud (https://sonarcloud.io) -----
        SONAR_HOST_URL    = 'https://sonarcloud.io'
        SONAR_ORG         = 'anhquan0402au-a11y'
        SONAR_PROJECT_KEY = 'anhquan0402au-a11y_223_7_3HD'
    }

    options {
        timestamps()
        buildDiscarder(logRotator(numToKeepStr: '15'))
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
                sh 'echo "Building commit:" && git rev-parse --short HEAD'
            }
        }

        // ---------- STAGE 1: BUILD ----------
        stage('Build') {
            steps {
                sh 'dotnet restore src/OrderApi/OrderApi.csproj'
                sh 'dotnet build src/OrderApi/OrderApi.csproj -c Release --no-restore'
                // Build a versioned Docker image + a latest tag
                sh "docker build -t ${IMAGE_NAME}:${IMAGE_TAG} -t ${IMAGE_NAME}:latest ."
                sh "docker images ${IMAGE_NAME}"
            }
        }

        // ---------- STAGE 2: TEST ----------
        stage('Test') {
            steps {
                sh 'rm -rf TestResults'
                sh 'dotnet test tests/OrderApi.Tests/OrderApi.Tests.csproj -c Release --logger "junit;LogFilePath=TestResults/test-results.xml" --collect:"XPlat Code Coverage"'
            }
            post {
                always {
                    junit allowEmptyResults: true, testResults: '**/TestResults/test-results.xml'
                }
            }
        }

        // ---------- STAGE 3: CODE QUALITY (SonarCloud) ----------
        stage('Code Quality') {
            steps {
                withCredentials([string(credentialsId: 'sonarcloud-token', variable: 'SONAR_TOKEN')]) {
                    sh '''
                        export PATH="$PATH:/root/.dotnet/tools"
                        dotnet tool install --global dotnet-sonarscanner || true

                        dotnet sonarscanner begin \
                          /k:"${SONAR_PROJECT_KEY}" \
                          /o:"${SONAR_ORG}" \
                          /d:sonar.host.url="${SONAR_HOST_URL}" \
                          /d:sonar.token="${SONAR_TOKEN}" \
                          /d:sonar.qualitygate.wait=true

                        dotnet build src/OrderApi/OrderApi.csproj -c Release --no-incremental

                        dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
                    '''
                }
            }
        }

        // ---------- STAGE 4: SECURITY ----------
        stage('Security') {
            steps {
                sh 'trivy --version'
                // Scan project dependencies (source tree)
                sh 'trivy fs --scanners vuln --severity HIGH,CRITICAL --exit-code 0 --format table -o trivy-fs-report.txt .'
                // Scan the built Docker image
                sh "trivy image --severity HIGH,CRITICAL --exit-code 0 --format table -o trivy-image-report.txt ${IMAGE_NAME}:${IMAGE_TAG}"
                sh 'echo "===== Trivy dependency scan =====" && cat trivy-fs-report.txt'
                sh 'echo "===== Trivy image scan =====" && cat trivy-image-report.txt'
            }
            post {
                always {
                    archiveArtifacts artifacts: 'trivy-*-report.txt', allowEmptyArchive: true
                }
            }
        }

        // ---------- STAGE 5: DEPLOY (staging) ----------
        stage('Deploy') {
            steps {
                sh "IMAGE=${IMAGE_NAME}:${IMAGE_TAG} docker compose -f docker-compose.staging.yml up -d"
                sh 'sleep 6'
                // Smoke test the staging container from the host (fails the build if unhealthy)
                sh 'curl -f http://host.docker.internal:8081/health'
            }
        }

        // ---------- STAGE 6: RELEASE (production) ----------
        stage('Release') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'dockerhub-creds', usernameVariable: 'DH_USER', passwordVariable: 'DH_PASS')]) {
                    sh '''
                        echo "$DH_PASS" | docker login -u "$DH_USER" --password-stdin

                        docker tag ${IMAGE_NAME}:${IMAGE_TAG} $DH_USER/${IMAGE_NAME}:${IMAGE_TAG}
                        docker tag ${IMAGE_NAME}:${IMAGE_TAG} $DH_USER/${IMAGE_NAME}:latest
                        docker push $DH_USER/${IMAGE_NAME}:${IMAGE_TAG}
                        docker push $DH_USER/${IMAGE_NAME}:latest

                        docker network inspect ${DOCKER_NETWORK} >/dev/null 2>&1 || docker network create ${DOCKER_NETWORK}

                        IMAGE=$DH_USER/${IMAGE_NAME}:${IMAGE_TAG} docker compose -f docker-compose.prod.yml up -d
                    '''
                    sh 'sleep 6'
                    sh 'curl -f http://host.docker.internal:8080/health'
                }
            }
        }

        // ---------- STAGE 7: MONITORING ----------
        stage('Monitoring') {
            steps {
                sh '''
                    docker network inspect ${DOCKER_NETWORK} >/dev/null 2>&1 || docker network create ${DOCKER_NETWORK}
                    docker compose -f monitoring/docker-compose.monitoring.yml up -d
                    sleep 6
                    curl -f http://host.docker.internal:9090/-/healthy
                    curl -f http://host.docker.internal:3000/api/health
                '''
            }
        }
    }

    post {
        success {
            echo 'Pipeline completed successfully. All stages green.'
        }
        failure {
            echo 'Pipeline failed - check the stage logs above.'
        }
    }
}

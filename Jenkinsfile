pipeline {

    agent {
        label 'dotnet'
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
    }

    environment {
        IMAGE_NAME = 'myapi'
        TEST_CONTAINER = 'myapi-test'
        TEST_PORT = '5000'
    }

    stages {

        // =========================================================
        // CHECKOUT
        // =========================================================

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // =========================================================
        // BUILD
        // =========================================================

        stage('Build') {
            steps {
                sh '''
                    echo "========================================"
                    echo "BUILD"
                    echo "========================================"

                    dotnet restore MyAPI.slnx

                    dotnet build MyAPI.slnx \
                        --configuration Release \
                        --no-restore
                '''
            }
        }

        // =========================================================
        // CODE QUALITY
        // =========================================================

        stage('Code Quality') {
            steps {
                sh '''
                    echo "========================================"
                    echo "CODE QUALITY"
                    echo "========================================"

                    dotnet format MyAPI.slnx \
                        --verify-no-changes \
                        --no-restore
                '''
            }
        }

        // =========================================================
        // UNIT TEST
        // =========================================================

        stage('Unit Tests') {
            steps {
                sh '''
                    echo "========================================"
                    echo "UNIT TESTS"
                    echo "========================================"

                    dotnet test MyAPI.slnx \
                        --configuration Release \
                        --no-build
                '''
            }
        }

        // =========================================================
        // DOCKER BUILD FOR TEST
        // =========================================================

        stage('Docker Build - Test') {
            steps {
                sh '''
                    echo "========================================"
                    echo "DOCKER BUILD - TEST"
                    echo "========================================"

                    docker build \
                        -t ${IMAGE_NAME}:test-${BUILD_NUMBER} \
                        -f src/WebApplication1/Dockerfile \
                        src/WebApplication1
                '''
            }
        }

        // =========================================================
        // START API CONTAINER
        // =========================================================

        stage('Start API - Test') {
            steps {
                sh '''
                    echo "========================================"
                    echo "START API TEST CONTAINER"
                    echo "========================================"

                    docker rm -f ${TEST_CONTAINER} || true

                    docker run -d \
                        --name ${TEST_CONTAINER} \
                        -p ${TEST_PORT}:8080 \
                        ${IMAGE_NAME}:test-${BUILD_NUMBER}

                    echo "Waiting for container..."

                    sleep 5

                    echo "========== CONTAINER STATUS =========="

                    docker ps -a

                    echo "========== CONTAINER LOGS =========="

                    docker logs ${TEST_CONTAINER} || true
                '''
            }
        }

        // =========================================================
        // WAIT FOR API
        // =========================================================

        stage('Wait For API') {
            steps {
                sh '''
                    echo "========================================"
                    echo "WAIT FOR API"
                    echo "========================================"

                    for i in $(seq 1 30)
                    do
                        echo "Attempt $i/30..."

                        if curl -fsS http://localhost:${TEST_PORT}/health
                        then
                            echo "API is ready."
                            exit 0
                        fi

                        sleep 2
                    done

                    echo "========================================"
                    echo "API FAILED TO START"
                    echo "========================================"

                    echo "========== DOCKER PS =========="

                    docker ps -a

                    echo "========== CONTAINER LOGS =========="

                    docker logs ${TEST_CONTAINER} || true

                    echo "========== CONTAINER INSPECT =========="

                    docker inspect ${TEST_CONTAINER} \
                        --format='Status={{.State.Status}} ExitCode={{.State.ExitCode}}'

                    exit 1
                '''
            }
        }

        // =========================================================
        // BDD / REQNROLL
        // =========================================================

        stage('BDD Tests - Reqnroll') {
            steps {
                sh '''
                    echo "========================================"
                    echo "BDD TESTS - REQNROLL"
                    echo "========================================"

                    dotnet test \
                        tests/MyAPI.BddTests/MyAPI.BddTests.csproj \
                        --configuration Release
                '''
            }
        }

        // =========================================================
        // DEVELOP -> STAGE
        // =========================================================

        stage('Develop -> Stage') {
            when {
                branch 'develop'
            }

            steps {
                sh '''
                    echo "========================================"
                    echo "DOCKER BUILD - DEVELOP"
                    echo "========================================"

                    docker build \
                        -t ${IMAGE_NAME}:develop-${BUILD_NUMBER} \
                        -t ${IMAGE_NAME}:stage \
                        -f src/WebApplication1/Dockerfile \
                        src/WebApplication1
                '''
            }
        }

        // =========================================================
        // MASTER -> PRODUCTION
        // =========================================================

        stage('Master -> Production') {
            when {
                branch 'master'
            }

            steps {
                sh '''
                    echo "========================================"
                    echo "DOCKER BUILD - MASTER"
                    echo "========================================"

                    docker build \
                        -t ${IMAGE_NAME}:master-${BUILD_NUMBER} \
                        -t ${IMAGE_NAME}:production \
                        -f src/WebApplication1/Dockerfile \
                        src/WebApplication1
                '''
            }
        }
    }

    // =============================================================
    // POST
    // =============================================================

    post {

        always {

            sh '''
                echo "========================================"
                echo "CLEANUP"
                echo "========================================"

                echo "Container logs before cleanup:"

                docker logs ${TEST_CONTAINER} || true

                echo "Removing test container..."

                docker rm -f ${TEST_CONTAINER} || true

                echo "Removing test image..."

                docker image rm \
                    ${IMAGE_NAME}:test-${BUILD_NUMBER} \
                    || true
            '''

            cleanWs()
        }

        success {
            echo '''
========================================
MYAPI PIPELINE SUCCESS
========================================
'''
        }

        failure {
            echo '''
========================================
MYAPI PIPELINE FAILED
========================================
'''
        }
    }
}

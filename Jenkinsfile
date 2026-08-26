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

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

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

                    docker ps
                '''
            }
        }

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

                    echo "API did not become ready."

                    docker logs ${TEST_CONTAINER}

                    exit 1
                '''
            }
        }

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

    post {

        always {
            sh '''
                echo "========================================"
                echo "CLEANUP"
                echo "========================================"

                docker logs ${TEST_CONTAINER} || true

                docker rm -f ${TEST_CONTAINER} || true

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

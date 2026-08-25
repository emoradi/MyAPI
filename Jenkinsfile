pipeline {
    agent {
        label 'dotnet'
    }

    environment {
        IMAGE_NAME = 'myapi'
    }

    options {
        timestamps()
        skipDefaultCheckout(true)
        disableConcurrentBuilds()
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

        stage('Test') {
            steps {
                sh '''
                    echo "========================================"
                    echo "TEST"
                    echo "========================================"

                    echo "---- Code Cleanup Check ----"

                    dotnet format MyAPI.slnx \
                        --verify-no-changes \
                        --no-restore

                    echo "Code cleanup check passed."

                    echo "---- Unit Tests ----"

                    dotnet test MyAPI.slnx \
                        --configuration Release \
                        --no-build \
                        --logger "console;verbosity=normal"
                '''
            }
        }

        stage('Develop -> Stage') {
            when {
                branch 'develop'
            }

            steps {
                script {
                    echo "========================================"
                    echo "DOCKER BUILD - DEVELOP"
                    echo "========================================"

                    sh '''
                        docker build \
                            -t ${IMAGE_NAME}:develop-${BUILD_NUMBER} \
                            -t ${IMAGE_NAME}:stage \
                            -f src/WebApplication1/Dockerfile \
                            src/WebApplication1
                    '''
                }
            }
        }

        stage('Master -> Production') {
            when {
                branch 'master'
            }

            steps {
                script {
                    echo "========================================"
                    echo "DOCKER BUILD - MASTER"
                    echo "========================================"

                    sh '''
                        docker build \
                            -t ${IMAGE_NAME}:master-${BUILD_NUMBER} \
                            -t ${IMAGE_NAME}:production \
                            -f src/WebApplication1/Dockerfile \
                            src/WebApplication1
                    '''
                }
            }
        }

        stage('Verify Docker Image') {
            when {
                anyOf {
                    branch 'develop'
                    branch 'master'
                }
            }

            steps {
                sh '''
                    echo "========================================"
                    echo "VERIFY DOCKER IMAGE"
                    echo "========================================"

                    docker images ${IMAGE_NAME}

                    if [ "${BRANCH_NAME}" = "develop" ]; then
                        docker image inspect ${IMAGE_NAME}:stage
                    fi

                    if [ "${BRANCH_NAME}" = "master" ]; then
                        docker image inspect ${IMAGE_NAME}:production
                    fi
                '''
            }
        }
    }

    post {
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

        always {
            cleanWs()
        }
    }
}

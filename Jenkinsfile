pipeline {

    agent {
        label 'myapi-dotnet9'
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout()
    }

    environment {
        PROJECT_NAME = 'myapi'
        PROJECT_PATH = 'src/WebApplication1'
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

                    dotnet test MyAPI.slnx \
                        --configuration Release \
                        --no-build
                '''
            }
        }

        stage('Develop -> Stage') {

            when {
                branch 'develop'
            }

            steps {

                script {

                    def imageVersion =
                        "${PROJECT_NAME}:develop-${BUILD_NUMBER}"

                    sh """
                        echo "========================================"
                        echo "DOCKER BUILD - DEVELOP"
                        echo "========================================"

                        docker build \
                            -t ${imageVersion} \
                            -t ${PROJECT_NAME}:stage \
                            -f ${PROJECT_PATH}/Dockerfile \
                            ${PROJECT_PATH}
                    """

                }
            }
        }

        stage('Verify Docker Image') {

            when {
                branch 'develop'
            }

            steps {

                sh '''
                    echo "========================================"
                    echo "DOCKER IMAGES"
                    echo "========================================"

                    docker images myapi

                    echo ""

                    echo "========================================"
                    echo "VERIFY STAGE IMAGE"
                    echo "========================================"

                    docker image inspect myapi:stage
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

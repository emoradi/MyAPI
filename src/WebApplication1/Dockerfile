pipeline {
    agent {
        label 'dotnet'
    }

    options {
        timestamps()
    }

    environment {
        IMAGE_NAME = 'myapi'
        IMAGE_TAG = "${BUILD_NUMBER}"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm

                sh '''
                    echo "Branch:"
                    git branch --show-current

                    echo "Commit:"
                    git rev-parse --short HEAD

                    echo "Files:"
                    ls -la
                '''
            }
        }

        stage('Restore') {
            steps {
                sh '''
                    dotnet restore
                '''
            }
        }

        stage('Build') {
            steps {
                sh '''
                    dotnet build \
                        --configuration Release \
                        --no-restore
                '''
            }
        }

        stage('Test') {
            steps {
                sh '''
                    dotnet test \
                        --configuration Release \
                        --no-build \
                        --verbosity normal
                '''
            }
        }

        stage('Docker Build') {
            steps {
                sh '''
                    docker build \
                        --tag ${IMAGE_NAME}:${IMAGE_TAG} \
                        --tag ${IMAGE_NAME}:latest \
                        .
                '''
            }
        }

        stage('Verify Docker Image') {
            steps {
                sh '''
                    docker images ${IMAGE_NAME}
                '''
            }
        }
    }

    post {
        success {
            echo 'CI Pipeline completed successfully.'
        }

        failure {
            echo 'CI Pipeline failed.'
        }

        always {
            sh '''
                echo "Docker images:"
                docker images
            '''
        }
    }
}

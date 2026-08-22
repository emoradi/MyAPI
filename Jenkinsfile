pipeline {

    agent {
        label 'myapi-dotnet9'
    }

    options {
        timestamps()
        disableConcurrentBuilds()
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Environment') {
            steps {
                sh '''
                    echo "========================================"
                    echo "Environment"
                    echo "========================================"

                    echo ""
                    echo "Hostname:"
                    hostname

                    echo ""
                    echo "Git:"
                    git --version

                    echo ""
                    echo ".NET:"
                    dotnet --info
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
                        --no-build
                '''
            }
        }
    }

    post {
        success {
            echo '========================================'
            echo 'CI SUCCESS'
            echo '========================================'
        }

        failure {
            echo '========================================'
            echo 'CI FAILED'
            echo '========================================'
        }

        always {
            cleanWs()
        }
    }
}

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
                    echo "ENVIRONMENT"
                    echo "========================================"

                    echo "Hostname:"
                    hostname

                    echo ""
                    echo "User:"
                    whoami

                    echo ""
                    echo "Working Directory:"
                    pwd

                    echo ""
                    echo "Git:"
                    git --version

                    echo ""
                    echo ".NET:"
                    dotnet --info
                '''
            }
        }

        stage('List Source') {
            steps {
                sh '''
                    echo "========================================"
                    echo "SOURCE TREE"
                    echo "========================================"

                    find . -maxdepth 3 -type f | sort
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
    }

    post {

        success {
            echo '''
========================================
MYAPI CI SUCCESS
========================================
'''
        }

        failure {
            echo '''
========================================
MYAPI CI FAILED
========================================
'''
        }

        always {
            cleanWs()
        }
    }
}
pipeline {
    agent {
        label 'myapi-dotnet9'
    }

    stages {

        stage('Agent Test') {
            steps {
                sh 'whoami'
                sh 'uname -a'
                sh 'java -version'
                sh 'dotnet --version'
                sh 'git --version'
                sh 'docker --version'
            }
        }

        stage('Docker Test') {
            steps {
                sh 'docker info'
            }
        }

    }
}

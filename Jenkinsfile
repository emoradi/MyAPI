pipeline {
    agent any

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build') {
            steps {
                echo 'Building MyAPI...'
            }
        }

        stage('Test') {
            steps {
                echo 'Testing MyAPI....'
            }
        }
    }
}
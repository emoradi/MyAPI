pipeline {

    agent {
        label 'myapi-dotnet9'
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
                    echo "=== HOSTNAME ==="
                    hostname

                    echo "=== GIT ==="
                    git --version

                    echo "=== DOTNET ==="
                    dotnet --info

                    echo "=== JAVA ==="
                    java -version
                '''
            }
        }

        stage('Restore') {
            steps {
                dir('src/WebApplication1') {
                    sh '''
                        dotnet restore WebApplication1.csproj
                    '''
                }
            }
        }

        stage('Build') {
            steps {
                dir('src/WebApplication1') {
                    sh '''
                        dotnet build WebApplication1.csproj \
                            --configuration Release \
                            --no-restore
                    '''
                }
            }
        }
    }
}

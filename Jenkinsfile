pipeline {

    agent any

    options {
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
        timestamps()
    }

    environment {

        // Docker image stored locally
        IMAGE_NAME = 'myapi'

        // Kubernetes
        STAGE_NAMESPACE      = 'stage'
        PRODUCTION_NAMESPACE = 'production'

        DEPLOYMENT_NAME = 'myapi'
        CONTAINER_NAME  = 'myapi'
    }

    stages {

        // =====================================================
        // 1. Checkout
        // =====================================================

        stage('Checkout') {

            steps {

                checkout scm

                sh '''
                    echo "========================================"
                    echo "Repository"
                    echo "========================================"

                    git remote -v

                    echo ""
                    echo "Commit:"
                    git rev-parse HEAD

                    echo ""
                    echo "Branch:"
                    git branch --show-current

                    echo ""
                    echo "Tag:"
                    git describe --tags --exact-match HEAD 2>/dev/null || true
                '''
            }
        }


        // =====================================================
        // 2. Determine Pipeline Type
        // =====================================================

        stage('Determine Pipeline Type') {

            steps {

                script {

                    echo "BRANCH_NAME = ${env.BRANCH_NAME ?: 'N/A'}"
                    echo "TAG_NAME    = ${env.TAG_NAME ?: 'N/A'}"
                    echo "CHANGE_ID   = ${env.CHANGE_ID ?: 'N/A'}"


                    // -----------------------------------------
                    // Pull Request
                    // -----------------------------------------

                    if (env.CHANGE_ID) {

                        env.PIPELINE_TYPE = 'CI'
                    }


                    // -----------------------------------------
                    // Stage Release
                    //
                    // stage-v1.0.0
                    // stage-v1.2.3
                    // -----------------------------------------

                    else if (
                        env.TAG_NAME &&
                        env.TAG_NAME ==~ /^stage-v[0-9]+\.[0-9]+\.[0-9]+$/
                    ) {

                        env.PIPELINE_TYPE = 'STAGE'

                        env.VERSION =
                            env.TAG_NAME.replace('stage-v', '')
                    }


                    // -----------------------------------------
                    // Production Release
                    //
                    // v1.0.0
                    // v1.2.3
                    // -----------------------------------------

                    else if (
                        env.TAG_NAME &&
                        env.TAG_NAME ==~ /^v[0-9]+\.[0-9]+\.[0-9]+$/
                    ) {

                        env.PIPELINE_TYPE = 'PRODUCTION'

                        env.VERSION =
                            env.TAG_NAME.replace('v', '')
                    }


                    // -----------------------------------------
                    // develop / master
                    // -----------------------------------------

                    else if (
                        env.BRANCH_NAME == 'develop' ||
                        env.BRANCH_NAME == 'master'
                    ) {

                        env.PIPELINE_TYPE = 'CI'
                    }


                    // -----------------------------------------
                    // Unsupported branch/tag
                    // -----------------------------------------

                    else {

                        error(
                            "Unsupported branch/tag. " +
                            "BRANCH=${env.BRANCH_NAME}, " +
                            "TAG=${env.TAG_NAME}"
                        )
                    }


                    echo ""
                    echo "========================================"
                    echo "Pipeline Type : ${env.PIPELINE_TYPE}"
                    echo "Version       : ${env.VERSION ?: 'N/A'}"
                    echo "========================================"
                }
            }
        }


        // =====================================================
        // 3. Restore
        // =====================================================

        stage('Restore') {

            steps {

                dir('src/WebApplication1') {

                    sh '''
                        dotnet restore WebApplication1.csproj
                    '''
                }
            }
        }


        // =====================================================
        // 4. Build
        // =====================================================

        stage('Build') {

            steps {

                dir('src/WebApplication1') {

                    sh '''
                        dotnet build \
                            WebApplication1.csproj \
                            --configuration Release \
                            --no-restore
                    '''
                }
            }
        }


        // =====================================================
        // 5. Test
        // =====================================================

        stage('Test') {

            steps {

                echo "No test project exists currently."

            }
        }


        // =====================================================
        // 6. Docker Build
        //
        // ONLY Stage
        // =====================================================

        stage('Docker Build') {

            when {

                expression {
                    env.PIPELINE_TYPE == 'STAGE'
                }
            }

            steps {

                dir('src/WebApplication1') {

                    sh """

                        echo "Building Docker image..."

                        docker build \
                            -t ${IMAGE_NAME}:${VERSION} \
                            .

                        echo ""
                        echo "Docker images:"
                        docker images ${IMAGE_NAME}

                    """
                }
            }
        }


        // =====================================================
        // 7. Verify Local Docker Image
        // =====================================================

        stage('Verify Docker Image') {

            when {

                anyOf {

                    expression {
                        env.PIPELINE_TYPE == 'STAGE'
                    }

                    expression {
                        env.PIPELINE_TYPE == 'PRODUCTION'
                    }
                }
            }

            steps {

                script {

                    def imageExists = sh(
                        script: """
                            docker image inspect \
                                ${IMAGE_NAME}:${VERSION}
                        """,
                        returnStatus: true
                    )

                    if (imageExists != 0) {

                        error(
                            "Docker image not found locally: " +
                            "${IMAGE_NAME}:${VERSION}"
                        )
                    }

                    echo ""
                    echo "========================================"
                    echo "Docker Image Found"
                    echo "========================================"
                    echo "${IMAGE_NAME}:${VERSION}"
                    echo "========================================"
                }
            }
        }


        // =====================================================
        // 8. Deploy Stage
        // =====================================================

        stage('Deploy Stage') {

            when {

                expression {
                    env.PIPELINE_TYPE == 'STAGE'
                }
            }

            steps {

                sh """

                    echo "Deploying to Stage..."

                    kubectl -n ${STAGE_NAMESPACE} \
                        set image deployment/${DEPLOYMENT_NAME} \
                        ${CONTAINER_NAME}=${IMAGE_NAME}:${VERSION}

                    kubectl -n ${STAGE_NAMESPACE} \
                        rollout status deployment/${DEPLOYMENT_NAME} \
                        --timeout=5m

                """
            }
        }


        // =====================================================
        // 9. Deploy Production
        //
        // No Docker Build
        // No Docker Push
        // =====================================================

        stage('Deploy Production') {

            when {

                expression {
                    env.PIPELINE_TYPE == 'PRODUCTION'
                }
            }

            steps {

                sh """

                    echo "Deploying to Production..."

                    kubectl -n ${PRODUCTION_NAMESPACE} \
                        set image deployment/${DEPLOYMENT_NAME} \
                        ${CONTAINER_NAME}=${IMAGE_NAME}:${VERSION}

                    kubectl -n ${PRODUCTION_NAMESPACE} \
                        rollout status deployment/${DEPLOYMENT_NAME} \
                        --timeout=5m

                """
            }
        }
    }


    // =========================================================
    // Post
    // =========================================================

    post {

        success {

            echo """
            ========================================
            PIPELINE SUCCESS
            ========================================

            Pipeline Type : ${env.PIPELINE_TYPE}
            Branch        : ${env.BRANCH_NAME ?: 'N/A'}
            Tag           : ${env.TAG_NAME ?: 'N/A'}
            Version       : ${env.VERSION ?: 'N/A'}
            Image         : ${env.IMAGE_NAME}:${env.VERSION ?: 'N/A'}

            ========================================
            """
        }


        failure {

            echo """
            ========================================
            PIPELINE FAILED
            ========================================

            Pipeline Type : ${env.PIPELINE_TYPE ?: 'N/A'}
            Branch        : ${env.BRANCH_NAME ?: 'N/A'}
            Tag           : ${env.TAG_NAME ?: 'N/A'}

            ========================================
            """
        }


        always {

            echo "Cleaning workspace..."

            cleanWs()
        }
    }
}
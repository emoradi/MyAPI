pipeline {

    agent any

    options {
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
        timestamps()
    }

    environment {

        // =====================================================
        // Application
        // =====================================================

        IMAGE_NAME = 'myapi'

        // =====================================================
        // Kubernetes
        // =====================================================

        STAGE_NAMESPACE = 'stage'
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
                    echo "Git Information"
                    echo "========================================"

                    echo "Repository:"
                    git remote get-url origin

                    echo ""
                    echo "Commit:"
                    git rev-parse HEAD

                    echo ""
                    echo "Branch:"
                    echo "${BRANCH_NAME}"

                    echo ""
                    echo "Tag:"
                    echo "${TAG_NAME:-N/A}"

                    echo "========================================"
                '''
            }
        }


        // =====================================================
        // 2. Validate Environment
        // =====================================================

        stage('Validate Environment') {

            steps {

                sh '''
                    echo "========================================"
                    echo "Environment"
                    echo "========================================"

                    echo ""
                    echo "Git:"
                    git --version

                    echo ""
                    echo ".NET:"
                    dotnet --info

                    echo ""
                    echo "Docker:"
                    docker --version

                    echo ""
                    echo "kubectl:"
                    kubectl version --client

                    echo "========================================"
                '''
            }
        }


        // =====================================================
        // 3. Determine Pipeline Type
        // =====================================================

        stage('Determine Pipeline Type') {

            steps {

                script {

                    echo "BRANCH_NAME = ${env.BRANCH_NAME ?: 'N/A'}"
                    echo "TAG_NAME    = ${env.TAG_NAME ?: 'N/A'}"
                    echo "CHANGE_ID   = ${env.CHANGE_ID ?: 'N/A'}"


                    // =================================================
                    // Pull Request
                    // =================================================

                    if (env.CHANGE_ID) {

                        env.PIPELINE_TYPE = 'CI'
                    }


                    // =================================================
                    // Stage Tag
                    //
                    // Example:
                    //
                    // stage-v1.0.0
                    // stage-v1.2.3
                    // =================================================

                    else if (
                        env.TAG_NAME &&
                        env.TAG_NAME ==~ /^stage-v[0-9]+\.[0-9]+\.[0-9]+$/
                    ) {

                        env.PIPELINE_TYPE = 'STAGE'

                        env.VERSION =
                            env.TAG_NAME.replace('stage-v', '')
                    }


                    // =================================================
                    // Production Tag
                    //
                    // Example:
                    //
                    // v1.0.0
                    // v1.2.3
                    // =================================================

                    else if (
                        env.TAG_NAME &&
                        env.TAG_NAME ==~ /^v[0-9]+\.[0-9]+\.[0-9]+$/
                    ) {

                        env.PIPELINE_TYPE = 'PRODUCTION'

                        env.VERSION =
                            env.TAG_NAME.replace('v', '')
                    }


                    // =================================================
                    // develop / master
                    // =================================================

                    else if (
                        env.BRANCH_NAME == 'develop' ||
                        env.BRANCH_NAME == 'master'
                    ) {

                        env.PIPELINE_TYPE = 'CI'
                    }


                    // =================================================
                    // Unsupported branch/tag
                    // =================================================

                    else {

                        error(
                            "Unsupported branch/tag. " +
                            "BRANCH=${env.BRANCH_NAME}, " +
                            "TAG=${env.TAG_NAME}"
                        )
                    }


                    echo ""
                    echo "========================================"
                    echo "Pipeline Configuration"
                    echo "========================================"
                    echo "Pipeline Type : ${env.PIPELINE_TYPE}"
                    echo "Version       : ${env.VERSION ?: 'N/A'}"
                    echo "========================================"
                }
            }
        }


        // =====================================================
        // 4. Restore
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
        // 5. Build
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
        // 6. Test
        // =====================================================

        stage('Test') {

            steps {

                echo "No test project exists in the current repository."

            }
        }


        // =====================================================
        // 7. Docker Build
        //
        // ONLY Stage
        //
        // Docker image is stored locally.
        // No Docker Registry.
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

                        echo "========================================"
                        echo "Docker Build"
                        echo "========================================"

                        docker build \
                            --tag ${IMAGE_NAME}:${VERSION} \
                            .

                        echo ""
                        echo "Docker image created:"
                        docker images ${IMAGE_NAME}

                        echo "========================================"

                    """
                }
            }
        }


        // =====================================================
        // 8. Verify Docker Image
        //
        // Stage:
        // Image has just been created.
        //
        // Production:
        // Image must already exist locally.
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

                    echo "Checking Docker image..."

                    def result = sh(
                        script: """
                            docker image inspect \
                                ${IMAGE_NAME}:${VERSION}
                        """,
                        returnStatus: true
                    )

                    if (result != 0) {

                        error(
                            "Docker image does not exist locally: " +
                            "${IMAGE_NAME}:${VERSION}"
                        )
                    }

                    echo ""
                    echo "Docker image found:"
                    echo "${IMAGE_NAME}:${VERSION}"
                }
            }
        }


        // =====================================================
        // 9. Deploy Stage
        // =====================================================

        stage('Deploy Stage') {

            when {

                expression {
                    env.PIPELINE_TYPE == 'STAGE'
                }
            }

            steps {

                sh """

                    echo "========================================"
                    echo "Deploying to Stage"
                    echo "========================================"

                    kubectl \
                        --namespace ${STAGE_NAMESPACE} \
                        set image \
                        deployment/${DEPLOYMENT_NAME} \
                        ${CONTAINER_NAME}=${IMAGE_NAME}:${VERSION}

                    kubectl \
                        --namespace ${STAGE_NAMESPACE} \
                        rollout status \
                        deployment/${DEPLOYMENT_NAME} \
                        --timeout=5m

                    echo "========================================"

                """
            }
        }


        // =====================================================
        // 10. Deploy Production
        //
        // IMPORTANT:
        //
        // No Docker Build
        // No Docker Push
        //
        // Uses existing local image.
        // =====================================================

        stage('Deploy Production') {

            when {

                expression {
                    env.PIPELINE_TYPE == 'PRODUCTION'
                }
            }

            steps {

                sh """

                    echo "========================================"
                    echo "Deploying to Production"
                    echo "========================================"

                    kubectl \
                        --namespace ${PRODUCTION_NAMESPACE} \
                        set image \
                        deployment/${DEPLOYMENT_NAME} \
                        ${CONTAINER_NAME}=${IMAGE_NAME}:${VERSION}

                    kubectl \
                        --namespace ${PRODUCTION_NAMESPACE} \
                        rollout status \
                        deployment/${DEPLOYMENT_NAME} \
                        --timeout=5m

                    echo "========================================"

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

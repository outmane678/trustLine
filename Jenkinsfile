pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO               = '1'
    }

    triggers {
        githubPush()
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore Project.sln'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build Project.sln --no-restore --configuration Release'
            }
        }

        stage('Unit Tests') {
            steps {
                bat 'dotnet test TrustLine.UnitTests/TrustLine.Tests.csproj --no-build --configuration Release --logger "trx;LogFileName=unit-tests.trx" --collect:"XPlat Code Coverage" --results-directory .\\TestResults\\Unit'
            }
            post {
                always {
                    junit 'TestResults/Unit/**/*.trx'
                }
            }
        }

        stage('Integration Tests') {
            steps {
                bat 'dotnet test TrustLine.IntegrationTests/TrustLine.IntegrationTests.csproj --no-build --configuration Release --logger "trx;LogFileName=integration-tests.trx" --collect:"XPlat Code Coverage" --results-directory .\\TestResults\\Integration'
            }
            post {
                always {
                    junit 'TestResults/Integration/**/*.trx'
                }
            }
        }

        stage('Publish Coverage') {
            steps {
                publishCoverage(
                    adapters: [
                        coberturaAdapter('TestResults/**/coverage.cobertura.xml')
                    ],
                    sourceFileResolver: sourceFiles('STORE_ALL_BUILD')
                )
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            echo 'Pipeline terminé avec succès.'
        }
        failure {
            echo 'Pipeline échoué — vérifier les logs.'
        }
    }
}

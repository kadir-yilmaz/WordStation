pipeline {
    agent none

    environment {
        // Teknik Parametreler (Git'e girmesinde sakınca olmayanlar)
        WEBAPI_SERVICE_URL = 'site7885.siteasp.net'
        WEBAPI_SITE_NAME   = 'site7885'
        
        WEBUI_SERVICE_URL  = 'site40040.siteasp.net'
        WEBUI_SITE_NAME    = 'site40040'
    }

    stages {
        stage('Restore') {
            agent { label 'master || built-in' }
            steps {
                echo '📦 Paketler geri yükleniyor (Linux Master)...'
                sh 'dotnet restore WordStation.sln'
            }
        }

        stage('Test') {
            agent { label 'master || built-in' }
            steps {
                echo '🧪 Testler çalıştırılıyor (Linux Master)...'
                sh 'dotnet test WordStation.Tests --no-restore -c Release'
            }
        }

        stage('Build') {
            agent { label 'master || built-in' }
            steps {
                echo '🏗️ Proje derleniyor (Linux Master)...'
                sh 'dotnet build WordStation.sln -c Release --no-restore'
            }
        }

        stage('Deploy WebAPI') {
            agent { label 'windows' }
            when {
                anyOf {
                    changeset "WordStation.WebAPI/**"
                    changeset "WordStation.EL/**"
                    changeset "WordStation.DAL/**"
                    changeset "WordStation.BLL/**"
                }
            }
            steps {
                withCredentials([usernamePassword(credentialsId: 'webapi-ftp', passwordVariable: 'FTP_PASS', usernameVariable: 'FTP_USER')]) {
                    echo '🚀 WebAPI yayınlanıyor (Fileless WebDeploy)...'
                    bat """
                        dotnet publish WordStation.WebAPI -c Release ^
                        /p:WebPublishMethod=MSDeploy ^
                        /p:MSDeployServiceURL=${WEBAPI_SERVICE_URL} ^
                        /p:DeployIisAppPath=${WEBAPI_SITE_NAME} ^
                        /p:UserName=${FTP_USER} ^
                        /p:Password=${FTP_PASS} ^
                        /p:MSDeployPublishMethod=WMSVC ^
                        /p:EnableMsDeployAppOffline=true ^
                        /p:AllowUntrustedCertificate=true
                    """
                }
            }
        }

        stage('Deploy WebUI') {
            agent { label 'windows' }
            when {
                anyOf {
                    changeset "WordStation.WebUI/**"
                    changeset "WordStation.EL/**"
                    changeset "WordStation.DAL/**"
                    changeset "WordStation.BLL/**"
                }
            }
            steps {
                withCredentials([usernamePassword(credentialsId: 'webui-ftp', passwordVariable: 'FTP_PASS', usernameVariable: 'FTP_USER')]) {
                    echo '🚀 WebUI yayınlanıyor (Fileless WebDeploy)...'
                    bat """
                        dotnet publish WordStation.WebUI -c Release ^
                        /p:WebPublishMethod=MSDeploy ^
                        /p:MSDeployServiceURL=${WEBUI_SERVICE_URL} ^
                        /p:DeployIisAppPath=${WEBUI_SITE_NAME} ^
                        /p:UserName=${FTP_USER} ^
                        /p:Password=${FTP_PASS} ^
                        /p:MSDeployPublishMethod=WMSVC ^
                        /p:EnableMsDeployAppOffline=true ^
                        /p:AllowUntrustedCertificate=true
                    """
                }
            }
        }
    }

    post {
        always {
            echo 'İşlem tamamlandı (Jenkins CI).'
        }
        success {
            echo '✅ Tebrikler! Tüm aşamalar "Fileless WebDeploy" ile başarıyla geçti.'
        }
        failure {
            echo '❌ Hata! Lütfen logları ve Windows Agent bağlantısını kontrol edin.'
        }
    }
}

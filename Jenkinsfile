pipeline {
    agent none

    environment {
        // Sunucu Adresleri
        WEBAPI_SERVER   = 'site7885.siteasp.net'
        WEBUI_SERVER    = 'site40040.siteasp.net'
    }

    stages {
        stage('Restore') {
            agent { label 'master' }
            steps {
                echo '📦 Paketler geri yükleniyor (Linux Master)...'
                sh 'dotnet restore WordStation.sln'
            }
        }

        stage('Test') {
            agent { label 'master' }
            steps {
                echo '🧪 Testler çalıştırılıyor (Linux Master)...'
                sh 'dotnet test WordStation.Tests --no-restore -c Release'
            }
        }

        stage('Build') {
            agent { label 'master' }
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
                    echo '🚀 WebAPI yayınlanıyor (Windows WebDeploy)...'
                    bat "dotnet publish WordStation.WebAPI -c Release /p:PublishProfile=site7885-WebDeploy /p:Password=${FTP_PASS} /p:AllowUntrustedCertificate=true"
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
                    echo '🚀 WebUI yayınlanıyor (Windows WebDeploy)...'
                    bat "dotnet publish WordStation.WebUI -c Release /p:PublishProfile=site40040-WebDeploy /p:Password=${FTP_PASS} /p:AllowUntrustedCertificate=true"
                }
            }
        }
    }

    post {
        always {
            echo 'İşlem tamamlandı (Jenkins CI).'
        }
        success {
            echo '✅ Tebrikler! Tüm aşamalar WebDeploy ile başarıyla geçti.'
        }
        failure {
            echo '❌ Hata! Lütfen logları ve Windows Agent bağlantısını kontrol edin.'
        }
    }
}

pipeline {
    agent any

    environment {
        // Jenkins içindeki .NET çalışma alanı
        DOTNET_CLI_HOME = "${WORKSPACE}/.dotnet"
        
        // Tüm hassas bilgiler Jenkins arayüzünden (Credentials ve Parameters) yönetilmelidir.
        FTP_AUTH = credentials('ftp-credentials')
        
        // WEBAPI_FTP_SERVER ve WEBAPI_REMOTE_DIR artık Jenkins Job ayarlarından atanmalıdır.
    }

    stages {
        stage('Restore') {
            steps {
                echo '📦 Paketler geri yükleniyor...'
                sh 'dotnet restore WordStation.sln'
            }
        }

        stage('Test') {
            steps {
                echo '🧪 Testler çalıştırılıyor...'
                sh 'dotnet test WordStation.Tests --no-restore -c Release'
            }
        }

        stage('Build') {
            steps {
                echo '🏗️ Proje derleniyor...'
                sh 'dotnet build WordStation.WebAPI -c Release --no-restore'
            }
        }

        stage('Deploy to Production (FTP)') {
            steps {
                echo '🚀 WebAPI yayınlanıyor...'
                sh 'dotnet publish WordStation.WebAPI -c Release -o ./publish/WebAPI'
                
                echo '📤 Dosyalar FTP sunucusuna aktarılıyor...'
                sh '''
                    lftp -c "set ftp:ssl-allow no; \
                    open -u ${FTP_AUTH_USR},${FTP_AUTH_PSW} ftp://${WEBAPI_FTP_SERVER}; \
                    mirror -R ./publish/WebAPI ${WEBAPI_REMOTE_DIR} --delete --verbose"
                '''
                
                echo '✅ Yayınlama tamamlandı.'
            }
        }
    }

    post {
        always {
            echo 'İşlem tamamlandı (Jenkins CI).'
        }
        success {
            echo '✅ Tebrikler! Tüm aşamalar başarıyla geçti.'
        }
        failure {
            echo '❌ Hata! Lütfen logları kontrol edin.'
        }
    }
}

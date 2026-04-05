pipeline {
    agent any

    environment {
        // Jenkins içindeki .NET çalışma alanı
        DOTNET_CLI_HOME = "${WORKSPACE}/.dotnet"
        
        // Sunucu Adresleri (Şifre içermez, kodda durabilir)
        WEBAPI_SERVER   = 'site7885.siteasp.net'
        WEBUI_SERVER    = 'site40040.siteasp.net'
        
        // Uzak Dizinler
        REMOTE_DIR      = 'wwwroot/' 
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
                sh 'dotnet build WordStation.sln -c Release --no-restore'
            }
        }

        stage('Deploy WebAPI') {
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
                    echo '🚀 WebAPI yayınlanıyor...'
                    sh 'dotnet publish WordStation.WebAPI -c Release -o ./publish/WebAPI'
                    
                    echo '📤 WebAPI dosyaları FTPS (Secure) ile aktarılıyor...'
                    sh '''
                        lftp <<EOF || true
                        debug 10
                        set ftp:ssl-force yes
                        set ssl:verify-certificate no
                        set ftp:passive-mode on
                        set ftp:charset utf-8
                        open $WEBAPI_SERVER
                        user $FTP_USER $FTP_PASS
                        mirror -R ./publish/WebAPI $REMOTE_DIR --no-perms --delete --verbose
                        quit
EOF
                    '''
                }
            }
        }

        stage('Deploy WebUI') {
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
                    echo '🚀 WebUI yayınlanıyor...'
                    sh 'dotnet publish WordStation.WebUI -c Release -o ./publish/WebUI'
                    
                    echo '📤 WebUI dosyaları FTPS (Secure) ile aktarılıyor...'
                    sh '''
                        lftp <<EOF || true
                        debug 10
                        set ftp:ssl-force yes
                        set ssl:verify-certificate no
                        set ftp:passive-mode on
                        set ftp:charset utf-8
                        open $WEBUI_SERVER
                        user $FTP_USER $FTP_PASS
                        mirror -R ./publish/WebUI $REMOTE_DIR --no-perms --delete --verbose
                        quit
EOF
                    '''
                }
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

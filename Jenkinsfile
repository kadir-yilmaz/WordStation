pipeline {
    agent any

    environment {
        // Jenkins içindeki .NET çalışma alanı
        DOTNET_CLI_HOME = "${WORKSPACE}/.dotnet"
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

        stage('Publish (Simulated Deploy)') {
            steps {
                echo '🚀 Yayınlanacak paketler oluşturuluyor...'
                sh 'dotnet publish WordStation.WebUI -c Release -o ./publish/WebUI'
                sh 'dotnet publish WordStation.WebAPI -c Release -o ./publish/WebAPI'
                
                echo 'Bilgi: Jenkins konteynerı Linux olduğu için IIS deploy bu aşamada simüle edilmiştir.'
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

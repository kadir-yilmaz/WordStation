# WordStation

ASP.NET Core (Web API & MVC) kullanılarak geliştirilmiş, gelişmiş mimari yapıları ve en iyi uygulama yöntemlerini içeren kapsamlı bir kelime öğrenme projesidir.

## Kullanılan Teknolojiler

- **Framework**: .NET 8.0
- **Architecture**: N-Layer Architecture (EL, BLL, DAL, WebUI, WebAPI)
- **ORM**: Entity Framework Core (SQL Server)
- **Database**: SQL Server
- **Mapping**: AutoMapper
- **Logging**: NLog
- **Security**: JWT (JSON Web Token) & ASP.NET Core Identity (Role Based Access Control)
- **UI**: ASP.NET Core MVC, Bootstrap 5, Bi-Icons, Custom CSS (Glassmorphism & Dark Theme)
- **Testing**: xUnit & Moq
- **CI/CD Süreçleri**: 
    - **GitHub Actions**: Her push ve pull request işleminde GitHub Secrets kullanılarak güvenli bir şekilde build ve birim test (xUnit) süreçleri otomatik hale getirilmiştir. Süreç sonunda elde edilen konfigürasyonlar appsettings.json dosyasına dinamik olarak enjekte edilerek dağıtım tamamlanmaktadır.
    - **Jenkins (Hibrit Mimari)**: Yerel makinede JAR tabanlı hibrit bir yapı ile kurgulanmıştır. Hassas veriler Jenkins Credentials üzerinden yönetilmekte; Linux master üzerinde gerçekleştirilen build ve test adımlarının ardından, Windows agent ve MSDeploy aracılığıyla proje production ortamına otomatik olarak deploy edilmektedir.

## Kurulum ve Çalıştırma

1. Projeyi klonlayın: `git clone https://github.com/kadir-yilmaz/WordStation.git`
2. `appsettings.json` dosyalarındaki veri tabanı bağlantı cümlelerini (ConnectionString) güncelleyin.
3. Package Manager Console üzerinden `update-database` komutunu çalıştırarak veri tabanı tablolarını oluşturun.
4. Projeyi çalıştırın.

---
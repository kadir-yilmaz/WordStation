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
- **Testing**: xUnit & Playwright (UI/E2E Testing)
- **CI/CD Süreçleri**: 
    - **GitHub Actions**: Akıllı Pipeline yapısı kurulmuştur. `paths` filtreleri sayesinde API değişimlerinde sadece API testleri ve deployment, UI değişimlerinde sadece UI testleri ve deployment tetiklenir.

## Test Altyapısı ve Çalıştırma

Projede hem iş mantığı (API) hem de kullanıcı deneyimi (UI) için kapsamlı test süreçleri bulunmaktadır.

### 1. API & Unit Testleri
İş mantığını ve servisleri test etmek için **xUnit** kullanılmıştır.
- **Çalıştırma**: `dotnet test WordStation.Tests`

### 2. UI (End-to-End) Testleri
Kullanıcı senaryolarını (Giriş, Kayıt vb.) gerçek tarayıcı üzerinde test etmek için **Playwright** kullanılmıştır.
- **Lokalde Çalıştırma (Detaylı Log ile)**:
  ```powershell
  dotnet test WordStation.UITests --logger "console;verbosity=detailed"
  ```
- **Görsel Mod (Visual Mode)**: `LoginTests.cs` veya `RegisterTests.cs` içinde `Headless = false` yaparak testlerin tarayıcıda koşturulmasını izleyebilirsiniz.

### 3. CI Pipeline Yapısı
GitHub Actions üzerinde testler iki ayrı kanaldan yürür:
- **Test-API**: DAL, BLL, WebAPI katmanlarındaki değişikliklerde tetiklenir.
- **Test-UI**: WebUI ve UITests katmanlarındaki değişikliklerde tetiklenir. UI pipeline'ı, uygulama arka planda (`http://localhost:5000`) ayağa kaldırarak testleri otomatik koşturur.

## Kurulum ve Çalıştırma

1. Projeyi klonlayın: `git clone https://github.com/kadir-yilmaz/WordStation.git`
2. `appsettings.json` dosyalarındaki veri tabanı bağlantı cümlelerini (ConnectionString) güncelleyin.
3. Package Manager Console üzerinden `update-database` komutunu çalıştırarak veri tabanı tablolarını oluşturun.
4. Playwright tarayıcılarını kurun (UI testleri için):
   ```powershell
   cd WordStation.UITests\bin\Debug\net10.0
   powershell -ExecutionPolicy Bypass .\playwright.ps1 install
   ```
5. Projeyi çalıştırın.

---
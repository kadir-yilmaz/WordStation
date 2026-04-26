using Microsoft.Playwright;
using Xunit;

namespace WordStation.UITests;

public class LoginTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private string BaseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "https://localhost:7042";

    public async Task InitializeAsync()
    {
        // GitHub Actions ortamında mıyız kontrol et?
        bool isCi = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = isCi, // CI ortamında true (görünmez), lokalde false (görünür)
            SlowMo = isCi ? 0 : 800 // CI ortamında hızlandır, lokalde izlemek için yavaşlat
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Login_ShouldShowError_WhenFieldsAreEmpty()
    {
        Console.WriteLine("\n[Login Test] Başlıyor: Boş alan kontrolü...");
        
        Console.WriteLine("--> Login sayfasına gidiliyor: " + $"{BaseUrl}/Account/Login");
        await _page.GotoAsync($"{BaseUrl}/Account/Login");

        Console.WriteLine("--> Hiçbir veri girmeden 'Giriş yap' butonuna basılıyor...");
        await _page.ClickAsync("button[type='submit']");

        Console.WriteLine("--> Hata mesajlarının görünürlüğü kontrol ediliyor...");
        var emailError = _page.Locator("span[data-valmsg-for='Email']");
        var passwordError = _page.Locator("span[data-valmsg-for='Password']");

        await Assertions.Expect(emailError).ToBeVisibleAsync();
        await Assertions.Expect(passwordError).ToBeVisibleAsync();
        Console.WriteLine("[BİTTİ] Boş alan hata mesajları başarıyla doğrulandı.");
    }

    [Fact]
    public async Task Login_ShouldShowError_WhenEmailIsInvalid()
    {
        Console.WriteLine("\n[Login Test] Başlıyor: Geçersiz email formatı kontrolü...");

        Console.WriteLine("--> Login sayfasına gidiliyor...");
        await _page.GotoAsync($"{BaseUrl}/Account/Login");

        Console.WriteLine("--> Email alanına geçersiz formatta metin giriliyor: 'gecersiz-email'");
        await _page.FillAsync("input[name='Email']", "gecersiz-email");
        await _page.FillAsync("input[name='Password']", "123456");
        
        Console.WriteLine("--> 'Giriş yap' butonuna basılıyor...");
        await _page.ClickAsync("button[type='submit']");

        Console.WriteLine("--> 'e-posta' uyarısının çıktığı kontrol ediliyor...");
        var emailError = _page.Locator("span[data-valmsg-for='Email']");
        await Assertions.Expect(emailError).ToContainTextAsync("e-posta");
        Console.WriteLine("[BİTTİ] Geçersiz email formatı uyarısı başarıyla doğrulandı.");
    }

    [Fact]
    public async Task Login_ShouldShowError_WhenCredentialsAreWrong()
    {
        Console.WriteLine("\n[Login Test] Başlıyor: Yanlış kullanıcı bilgileri kontrolü...");

        Console.WriteLine("--> Login sayfasına gidiliyor...");
        await _page.GotoAsync($"{BaseUrl}/Account/Login");

        Console.WriteLine("--> Sisteme kayıtlı olmayan bilgiler giriliyor: test@test.com");
        await _page.FillAsync("input[name='Email']", "test@test.com");
        await _page.FillAsync("input[name='Password']", "yanlis-sifre");
        
        Console.WriteLine("--> 'Giriş yap' butonuna basılıyor...");
        await _page.ClickAsync("button[type='submit']");

        Console.WriteLine("--> Sayfanın yönlenmediği ve hala Giriş sayfasında olduğumuz doğrulanıyor...");
        await Assertions.Expect(_page.Locator("body")).ToContainTextAsync("Giriş");
        Console.WriteLine("[BİTTİ] Yanlış bilgilerle giriş denemesi başarıyla engellendi.");
    }
}

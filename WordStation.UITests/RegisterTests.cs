using Microsoft.Playwright;
using Xunit;

namespace WordStation.UITests;

public class RegisterTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private string BaseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "https://localhost:7042";

    public async Task InitializeAsync()
    {
        bool isCi = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
        { 
            Headless = isCi, 
            SlowMo = isCi ? 0 : 800 
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Register_ShouldShowError_WhenPasswordsDoNotMatch()
    {
        Console.WriteLine("\n[Register Test] Başlıyor: Şifre uyuşmazlığı kontrolü...");

        Console.WriteLine("--> Register sayfasına gidiliyor...");
        await _page.GotoAsync($"{BaseUrl}/Account/Register");

        Console.WriteLine("--> Farklı şifreler giriliyor...");
        await _page.FillAsync("input[name='Email']", "yeni@user.com");
        await _page.FillAsync("input[name='Password']", "Sifre123!");
        await _page.FillAsync("input[name='ConfirmPassword']", "Sifre999!");

        Console.WriteLine("--> 'Hesabı Oluştur' butonuna basılıyor...");
        await _page.ClickAsync("button[type='submit']");

        Console.WriteLine("--> Şifre eşleşme hatasının çıktığı doğrulanıyor...");
        var confirmPasswordError = _page.Locator("span[data-valmsg-for='ConfirmPassword']");
        await Assertions.Expect(confirmPasswordError).ToBeVisibleAsync();
        Console.WriteLine("[BİTTİ] Şifre uyuşmazlığı hatası başarıyla doğrulandı.");
    }

    [Fact]
    public async Task Register_ShouldShowError_WhenEmailIsMissing()
    {
        Console.WriteLine("\n[Register Test] Başlıyor: Eksik email kontrolü...");

        Console.WriteLine("--> Register sayfasına gidiliyor...");
        await _page.GotoAsync($"{BaseUrl}/Account/Register");

        Console.WriteLine("--> Email boş bırakılıp şifreler dolduruluyor...");
        await _page.FillAsync("input[name='Password']", "Sifre123!");
        await _page.FillAsync("input[name='ConfirmPassword']", "Sifre123!");

        Console.WriteLine("--> 'Hesabı Oluştur' butonuna basılıyor...");
        await _page.ClickAsync("button[type='submit']");

        Console.WriteLine("--> Email zorunlu uyarısının çıktığı doğrulanıyor...");
        var emailError = _page.Locator("span[data-valmsg-for='Email']");
        await Assertions.Expect(emailError).ToBeVisibleAsync();
        Console.WriteLine("[BİTTİ] Eksik email hatası başarıyla doğrulandı.");
    }
}

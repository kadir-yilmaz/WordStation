using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace WordStation.WebUI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCustomApplicationCookie(this IServiceCollection services)
        {
            // Cookie ayarlarını burada yapılandırıyoruz
            var cookieBuilder = new CookieBuilder
            {
                Name = "WordStationAuth", // Kullanıcının isteği üzerine WordStationCookie de olabilir ama auth uyumu için mevcut ismi koruyalim veya değiştirelim. Kullanıcı kodunda "WordStationCookie" demiş.
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                SecurePolicy = CookieSecurePolicy.SameAsRequest
            };

            // Identity olmadığı için direkt AddAuthentication().AddCookie() zinciri kullanıyoruz.
            // Ancak bu metodun sadece ayarları yapılandırmasını istiyorsak Configure kullanabiliriz.
            // En temizi Program.cs'deki zinciri buraya taşımaktır.
            
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.LogoutPath = new PathString("/Account/Logout");
                    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                    options.Cookie = cookieBuilder;
                    
                    options.SlidingExpiration = false;
                    options.ExpireTimeSpan = TimeSpan.FromDays(2); 

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async context =>
                        {
                            if (context.Principal?.Identity?.IsAuthenticated == true)
                            {
                                var token = context.Principal.FindFirst("Token")?.Value;
                                var refreshToken = context.Principal.FindFirst("RefreshToken")?.Value;
                                var accessTokenExpStr = context.Principal.FindFirst("AccessTokenExpiration")?.Value;

                                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
                                {
                                    // Access Token expire süresine bak (Cookie süresine değil!)
                                    // RoundtripKind: ISO 8601 formatındaki 'Z' veya offset bilgisini doğru şekilde parse eder
                                    if (DateTime.TryParse(accessTokenExpStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var accessTokenExp))
                                    {
                                        // UTC'ye çevir (eğer Local ise)
                                        var accessTokenExpUtc = accessTokenExp.Kind == DateTimeKind.Local 
                                            ? accessTokenExp.ToUniversalTime() 
                                            : accessTokenExp;
                                            
                                        var now = DateTime.UtcNow;
                                        var timeRemaining = accessTokenExpUtc - now;
                                        
                                        // Debug log
                                        Console.WriteLine($"🔍 Token Check: Expire={accessTokenExpUtc:HH:mm:ss} Now={now:HH:mm:ss} Remaining={timeRemaining.TotalSeconds:F0}s");
                                        
                                        // Access Token 1 dakikadan az kaldıysa veya expire olduysa yenile
                                        if (timeRemaining.TotalMinutes < 1)
                                        {
                                            Console.WriteLine("⏰ Token refresh gerekli!");
                                            var authService = context.HttpContext.RequestServices.GetRequiredService<WordStation.WebUI.Services.Abstract.IAuthApiService>();
                                            var (success, data, error) = await authService.RefreshTokenAsync(token, refreshToken);

                                            if (success && data != null)
                                            {
                                                var identity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;
                                                
                                                // Token claim'lerini güncelle
                                                var tokenClaim = identity.FindFirst("Token");
                                                if (tokenClaim != null) identity.RemoveClaim(tokenClaim);
                                                identity.AddClaim(new System.Security.Claims.Claim("Token", data.Token));

                                                var refreshTokenClaim = identity.FindFirst("RefreshToken");
                                                if (refreshTokenClaim != null) identity.RemoveClaim(refreshTokenClaim);
                                                identity.AddClaim(new System.Security.Claims.Claim("RefreshToken", data.RefreshToken));

                                                // AccessTokenExpiration claim'ini güncelle
                                                var accExpClaim = identity.FindFirst("AccessTokenExpiration");
                                                if (accExpClaim != null) identity.RemoveClaim(accExpClaim);
                                                identity.AddClaim(new System.Security.Claims.Claim("AccessTokenExpiration", data.Expiration.ToString("o")));

                                                // Cookie'yi yeni claim'lerle yeniden yaz
                                                context.ShouldRenew = true;
                                                
                                                // Log: Token yenilendi
                                                Console.WriteLine($"🔄 Access Token otomatik yenilendi! Yeni expire: {data.Expiration:HH:mm:ss}");
                                            }
                                            else
                                            {
                                                // Refresh başarısız - oturumu sonlandır
                                                Console.WriteLine($"❌ Token yenileme başarısız: {error}");
                                                context.RejectPrincipal();
                                                await context.HttpContext.SignOutAsync();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };
                });
        }
        public static void ConfigureDataProtection(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var keysFolder = Path.Combine(environment.ContentRootPath, "Keys");
            
            services.AddDataProtection()
                .SetApplicationName("WordStationApp") // Uygulama adı
                .SetDefaultKeyLifetime(TimeSpan.FromDays(60)) // Anahtar ömrü
                .PersistKeysToFileSystem(new DirectoryInfo(keysFolder)); // Anahtarları klasöre kaydet
        }
    }
}

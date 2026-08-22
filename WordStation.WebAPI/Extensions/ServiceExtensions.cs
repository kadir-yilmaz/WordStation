using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WordStation.WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT Ayarlarını oku
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"❌ Auth Failure: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void AddCustomCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) return false;

                        try
                        {
                            var uri = new Uri(origin);
                            // Localhost / 127.0.0.1 geliştirme ortamları
                            if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            // Konfigürasyonda tanımlı izin verilen domainler
                            return allowedOrigins.Any(allowed =>
                                allowed.TrimEnd('/').Equals(origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // 🍪 Cookie ve Auth başlıklarının iletilmesi için zorunlu
                });
            });
        }

        public static void AddApplicationServices(this IServiceCollection services)
        {
        }
    }
}
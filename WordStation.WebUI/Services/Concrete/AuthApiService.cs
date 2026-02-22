using System.Text;
using System.Text.Json;
using WordStation.WebUI.Models;

using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Services.Concrete
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WordStationApi");
        }

        public async Task<(bool Success, TokenResponse? Data, string? Error)> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };
            var content = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync("auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage;
                    try 
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        // Try to parse { message: "..." }
                        using var doc = JsonDocument.Parse(errorJson);
                        if(doc.RootElement.TryGetProperty("message", out var msgElement))
                        {
                            errorMessage = msgElement.GetString();
                        }
                        else
                        {
                            errorMessage = errorJson; // fallback to raw
                        }
                    }
                    catch
                    {
                        errorMessage = response.ReasonPhrase ?? response.StatusCode.ToString();
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return (false, null, $"Login servisi bulunamadı (404). {errorMessage}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return (false, null, errorMessage ?? "Email veya şifre hatalı."); // API'den gelen mesajı kullan

                    return (false, null, $"Sunucu hatası ({response.StatusCode}): {errorMessage}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return (true, tokenResponse, null);
            }
            catch (Exception ex) // Bağlantı hatası vs.
            {
                return (false, null, $"Bağlantı hatası: {ex.Message}");
            }
        }

        public async Task<(bool Success, string[] Errors)> RegisterAsync(string email, string password)
        {
            var registerData = new { Email = email, Password = password, ConfirmPassword = password };
            var content = new StringContent(
                JsonSerializer.Serialize(registerData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("auth/register", content);

            if (response.IsSuccessStatusCode)
                return (true, Array.Empty<string>());

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return (false, errorResponse?.Errors ?? new[] { "Kayıt başarısız." });
            }
            catch
            {
                return (false, new[] { "Kayıt başarısız." });
            }
        }

        public async Task<(bool Success, TokenResponse? Data, string? Error)> RefreshTokenAsync(string token, string refreshToken)
        {
            var refreshData = new { Token = token, RefreshToken = refreshToken };
            var content = new StringContent(
                JsonSerializer.Serialize(refreshData),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync("auth/refresh-token", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage;
                    try
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(errorJson);
                        if (doc.RootElement.TryGetProperty("message", out var msgElement))
                        {
                            errorMessage = msgElement.GetString() ?? "Refresh failed";
                        }
                        else
                        {
                            errorMessage = errorJson;
                        }
                    }
                    catch
                    {
                        errorMessage = response.ReasonPhrase ?? "Unknown Error";
                    }

                    return (false, null, errorMessage);
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return (true, tokenResponse, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Bağlantı hatası: {ex.Message}");
            }
        }

        private class ErrorResponse
        {
            public string[]? Errors { get; set; }
            public string? Message { get; set; }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var revokeData = new { RefreshToken = refreshToken };
                var content = new StringContent(
                    JsonSerializer.Serialize(revokeData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("auth/revoke-token", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}

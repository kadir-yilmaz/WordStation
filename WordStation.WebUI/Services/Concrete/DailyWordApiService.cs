using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Services.Concrete
{
    public class DailyWordApiService : IDailyWordApiService
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DailyWordApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WordStationApi");
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, string token, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content != null) request.Content = content;
            return await _httpClient.SendAsync(request);
        }

        private StringContent ToJsonContent(object obj)
        {
            return new StringContent(
                JsonSerializer.Serialize(obj, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        public async Task<DailyWordSessionDto> GetSessionAsync(string userId, string listName, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Get,
                $"dailyword?userId={userId}&listName={listName}", token);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<DailyWordSessionDto> InitializeSessionAsync(string userId, string listName, string token)
        {
            var dto = new InitDailyWordSessionRequestDto { UserId = userId, ListName = listName };
            var response = await SendRequestAsync(HttpMethod.Post,
                "dailyword/init", token, ToJsonContent(dto));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<DailyWordSessionDto> AddToDailyAsync(string userId, string listName, List<int> wordIds, string token)
        {
            var dto = new AddToDailyRequestDto { UserId = userId, ListName = listName, WordIds = wordIds };
            var response = await SendRequestAsync(HttpMethod.Post,
                "dailyword/add", token, ToJsonContent(dto));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<DailyWordSessionDto> RemoveFromDailyAsync(string userId, string listName, List<int> wordIds, string token)
        {
            var dto = new RemoveFromDailyRequestDto { UserId = userId, ListName = listName, WordIds = wordIds };
            var response = await SendRequestAsync(HttpMethod.Post,
                "dailyword/remove", token, ToJsonContent(dto));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<DailyWordSessionDto> CompleteWordAsync(string userId, string listName, int wordId, string token)
        {
            var dto = new CompleteDailyWordRequestDto { UserId = userId, ListName = listName, WordId = wordId };
            var response = await SendRequestAsync(HttpMethod.Post,
                "dailyword/complete", token, ToJsonContent(dto));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<DailyWordSessionDto> CompleteWordsAsync(string userId, string listName, List<int> wordIds, string token)
        {
            var dto = new CompleteDailyWordsRequestDto { UserId = userId, ListName = listName, WordIds = wordIds };
            var response = await SendRequestAsync(HttpMethod.Post,
                "dailyword/complete-bulk", token, ToJsonContent(dto));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyWordSessionDto>(json, _jsonOptions);
        }

        public async Task<bool> DeleteSessionAsync(string userId, string listName, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Delete,
                $"dailyword?userId={userId}&listName={listName}", token);
            return response.IsSuccessStatusCode;
        }
    }
}

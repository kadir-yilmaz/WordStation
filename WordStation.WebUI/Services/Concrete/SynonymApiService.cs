#nullable enable
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Services.Concrete
{
    public class SynonymApiService : ISynonymApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public SynonymApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WordStationApi");
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, string token, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content != null) request.Content = content;
            return await _httpClient.SendAsync(request);
        }

        public async Task<IEnumerable<SynonymGroup>> GetAllGroupsAsync(string userId, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Get, $"synonymgroups?userId={userId}", token);

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<SynonymGroup>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<SynonymGroup>>(json, _jsonOptions)
                   ?? Enumerable.Empty<SynonymGroup>();
        }

        public async Task<IEnumerable<Word>> GetSynonymsForWordAsync(int wordId, string userId, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Get, $"synonymgroups/words/{wordId}/synonyms?userId={userId}", token);

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Word>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Word>>(json, _jsonOptions)
                   ?? Enumerable.Empty<Word>();
        }

        public async Task<Dictionary<int, IEnumerable<Word>>> GetAllSynonymsForUserAsync(string userId, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Get, $"synonymgroups/all-synonyms?userId={userId}", token);

            if (!response.IsSuccessStatusCode)
                return new Dictionary<int, IEnumerable<Word>>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<int, IEnumerable<Word>>>(json, _jsonOptions)
                   ?? new Dictionary<int, IEnumerable<Word>>();
        }

        public async Task<SynonymGroup?> CreateGroupAsync(string? name, List<int> wordIds, string userId, string token)
        {
            var requestBody = new { Name = name, WordIds = wordIds, UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await SendRequestAsync(HttpMethod.Post, "synonymgroups", token, content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SynonymGroup>(json, _jsonOptions);
        }

        public async Task<bool> AddWordToGroupAsync(int groupId, int wordId, string userId, string token)
        {
            var requestBody = new { WordId = wordId, UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await SendRequestAsync(HttpMethod.Post, $"synonymgroups/{groupId}/words", token, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveWordFromGroupAsync(int groupId, int wordId, string userId, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"synonymgroups/{groupId}/words/{wordId}?userId={userId}", token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGroupAsync(int groupId, string userId, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"synonymgroups/{groupId}?userId={userId}", token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateGroupNameAsync(int groupId, string? newName, string userId, string token)
        {
            var requestBody = new { Name = newName, UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await SendRequestAsync(HttpMethod.Patch, $"synonymgroups/{groupId}", token, content);
            return response.IsSuccessStatusCode;
        }
    }
}

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

        private void SetAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<IEnumerable<SynonymGroup>> GetAllGroupsAsync(string userId, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.GetAsync($"synonymgroups?userId={userId}");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<SynonymGroup>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<SynonymGroup>>(json, _jsonOptions) 
                   ?? Enumerable.Empty<SynonymGroup>();
        }

        public async Task<IEnumerable<Word>> GetSynonymsForWordAsync(int wordId, string userId, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.GetAsync($"synonymgroups/words/{wordId}/synonyms?userId={userId}");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Word>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Word>>(json, _jsonOptions) 
                   ?? Enumerable.Empty<Word>();
        }

        public async Task<SynonymGroup?> CreateGroupAsync(string? name, List<int> wordIds, string userId, string token)
        {
            SetAuthHeader(token);
            
            var request = new { Name = name, WordIds = wordIds, UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("synonymgroups", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SynonymGroup>(json, _jsonOptions);
        }

        public async Task<bool> AddWordToGroupAsync(int groupId, int wordId, string userId, string token)
        {
            SetAuthHeader(token);
            
            var request = new { WordId = wordId, UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"synonymgroups/{groupId}/words", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveWordFromGroupAsync(int groupId, int wordId, string userId, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.DeleteAsync($"synonymgroups/{groupId}/words/{wordId}?userId={userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGroupAsync(int groupId, string userId, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.DeleteAsync($"synonymgroups/{groupId}?userId={userId}");
            return response.IsSuccessStatusCode;
        }
    }
}

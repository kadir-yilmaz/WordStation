using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WordStation.WebUI.Models;

using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Services.Concrete
{
    public class WordApiService : IWordApiService
    {
        private readonly HttpClient _httpClient;

        public WordApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WordStationApi");
        }

        private void SetAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync(string userId, string listName, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.GetAsync($"words?userId={userId}&listName={listName}");
            
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Word>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Word>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<Word>();
        }

        public async Task<IEnumerable<Word>> SearchWordAsync(string en, string userId, string listName, string token, string searchMode = "starts")
        {
            SetAuthHeader(token);
            var response = await _httpClient.GetAsync($"words/search?en={en}&userId={userId}&listName={listName}&searchMode={searchMode}");
            
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Word>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Word>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<Word>();
        }

        public async Task<IEnumerable<string>> GetListNamesAsync(string userId, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.GetAsync($"words/lists?userId={userId}");
            
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<string>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<string>();
        }

        public async Task<bool> CreateWordAsync(Word word, string token)
        {
            SetAuthHeader(token);
            var content = new StringContent(
                JsonSerializer.Serialize(word),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("words", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateWordAsync(Word word, string token)
        {
            SetAuthHeader(token);
            var content = new StringContent(
                JsonSerializer.Serialize(word),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync("words", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWordAsync(int id, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.DeleteAsync($"words/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateListNameAsync(string userId, string listName, string newListName, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.PutAsync(
                $"words/lists/rename?userId={userId}&listName={listName}&newListName={newListName}",
                null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteListAsync(string userId, string listName, string token)
        {
            SetAuthHeader(token);
            var response = await _httpClient.DeleteAsync($"words/lists?userId={userId}&listName={listName}");
            return response.IsSuccessStatusCode;
        }
    }
}

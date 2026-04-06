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

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, string token, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content != null) request.Content = content;
            return await _httpClient.SendAsync(request);
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync(string userId, string listName, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Get, $"words?userId={userId}&listName={listName}", token);
            
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
            var response = await SendRequestAsync(HttpMethod.Get, $"words/search?en={en}&userId={userId}&listName={listName}&searchMode={searchMode}", token);
            
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
            var response = await SendRequestAsync(HttpMethod.Get, $"words/lists?userId={userId}", token);
            
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
            var content = new StringContent(
                JsonSerializer.Serialize(word),
                Encoding.UTF8,
                "application/json");

            var response = await SendRequestAsync(HttpMethod.Post, "words", token, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateWordAsync(Word word, string token)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(word),
                Encoding.UTF8,
                "application/json");

            var response = await SendRequestAsync(HttpMethod.Put, "words", token, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWordAsync(int id, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"words/{id}", token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateListNameAsync(string userId, string listName, string newListName, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Put, 
                $"words/lists/rename?userId={userId}&listName={listName}&newListName={newListName}", 
                token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteListAsync(string userId, string listName, string token)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, $"words/lists?userId={userId}&listName={listName}", token);
            return response.IsSuccessStatusCode;
        }
    }
}

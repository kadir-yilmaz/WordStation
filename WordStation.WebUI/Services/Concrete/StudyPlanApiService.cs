using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WordStation.EL.Dtos;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Services.Concrete
{
    public class StudyPlanApiService : IStudyPlanApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public StudyPlanApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WordStationApi");
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, string token, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            if (content != null)
            {
                request.Content = content;
            }
            return await _httpClient.SendAsync(request);
        }

        public async Task<List<StudyPlanSummaryDto>> GetUserPlansAsync(string userId, string token)
        {
            var url = string.IsNullOrWhiteSpace(userId) ? "studyplan" : $"studyplan?userId={Uri.EscapeDataString(userId)}";
            var response = await SendRequestAsync(HttpMethod.Get, url, token);

            if (!response.IsSuccessStatusCode)
                return new List<StudyPlanSummaryDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<StudyPlanSummaryDto>>(json, _jsonOptions) ?? new List<StudyPlanSummaryDto>();
        }

        public async Task<StudyPlanSummaryDto?> CreatePlanAsync(CreateStudyPlanDto dto, string token)
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await SendRequestAsync(HttpMethod.Post, "studyplan", token, content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StudyPlanSummaryDto>(json, _jsonOptions);
        }

        public async Task<bool> DeletePlanAsync(int planId, string userId, string token)
        {
            var url = string.IsNullOrWhiteSpace(userId)
                ? $"studyplan/{planId}"
                : $"studyplan/{planId}?userId={Uri.EscapeDataString(userId)}";

            var response = await SendRequestAsync(HttpMethod.Delete, url, token);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<StudyPlanDayDto>> GetAllDaysAsync(int planId, string userId, string token)
        {
            var url = string.IsNullOrWhiteSpace(userId)
                ? $"studyplan/{planId}/days"
                : $"studyplan/{planId}/days?userId={Uri.EscapeDataString(userId)}";

            var response = await SendRequestAsync(HttpMethod.Get, url, token);
            if (!response.IsSuccessStatusCode)
                return new List<StudyPlanDayDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<StudyPlanDayDto>>(json, _jsonOptions) ?? new List<StudyPlanDayDto>();
        }

        public async Task<StudyPlanDayDto?> GetDayAsync(int planId, int dayNumber, string userId, string token)
        {
            var url = string.IsNullOrWhiteSpace(userId)
                ? $"studyplan/{planId}/day/{dayNumber}"
                : $"studyplan/{planId}/day/{dayNumber}?userId={Uri.EscapeDataString(userId)}";

            var response = await SendRequestAsync(HttpMethod.Get, url, token);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StudyPlanDayDto>(json, _jsonOptions);
        }

        public async Task<StudyPlanQuizDto?> GenerateQuizAsync(int planId, int dayNumber, string userId, string token)
        {
            var url = string.IsNullOrWhiteSpace(userId)
                ? $"studyplan/{planId}/day/{dayNumber}/quiz"
                : $"studyplan/{planId}/day/{dayNumber}/quiz?userId={Uri.EscapeDataString(userId)}";

            var response = await SendRequestAsync(HttpMethod.Get, url, token);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StudyPlanQuizDto>(json, _jsonOptions);
        }

        public async Task<StudyPlanDayDto?> SubmitQuizAsync(int planId, int dayNumber, SubmitPlanQuizDto dto, string token)
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var url = $"studyplan/{planId}/day/{dayNumber}/quiz";
            var response = await SendRequestAsync(HttpMethod.Post, url, token, content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StudyPlanDayDto>(json, _jsonOptions);
        }
    }
}

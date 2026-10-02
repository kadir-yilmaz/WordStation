using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.WebUI.Models;

namespace WordStation.WebUI.Services.Abstract
{
    public interface IDailyWordApiService
    {
        Task<DailyWordSessionDto> GetSessionAsync(string userId, string listName, string token);
        Task<DailyWordSessionDto> InitializeSessionAsync(string userId, string listName, string token);
        Task<DailyWordSessionDto> AddToDailyAsync(string userId, string listName, List<int> wordIds, string token);
        Task<DailyWordSessionDto> RemoveFromDailyAsync(string userId, string listName, List<int> wordIds, string token);
        Task<DailyWordSessionDto> CompleteWordAsync(string userId, string listName, int wordId, string token);
        Task<DailyWordSessionDto> CompleteWordsAsync(string userId, string listName, List<int> wordIds, string token);
        Task<bool> DeleteSessionAsync(string userId, string listName, string token);
    }
}

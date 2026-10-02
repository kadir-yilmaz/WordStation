using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface IDailyWordService
    {
        Task<DailyWordSessionDto?> GetSessionAsync(string userId, string listName);
        Task<DailyWordSessionDto> InitializeSessionAsync(string userId, string listName);
        Task<DailyWordSessionDto> AddToDailyAsync(AddToDailyDto dto);
        Task<DailyWordSessionDto> RemoveFromDailyAsync(RemoveFromDailyDto dto);
        Task<DailyWordSessionDto> CompleteWordAsync(CompleteDailyWordDto dto);
        Task<DailyWordSessionDto> CompleteWordsAsync(CompleteDailyWordsDto dto);
        Task<bool> DeleteSessionAsync(string userId, string listName);
    }
}

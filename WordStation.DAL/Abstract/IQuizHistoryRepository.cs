using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IQuizHistoryRepository
    {
        Task<List<QuizHistory>> GetHistoryByUserIdAsync(string userId, bool? isDailyQuiz = null, int limit = 50, bool trackChanges = false);
        void CreateHistory(QuizHistory history);
        Task DeleteHistoryAsync(string userId, bool? isDailyQuiz = null);
        Task SaveAsync();
    }
}

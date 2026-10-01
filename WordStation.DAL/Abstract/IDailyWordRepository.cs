using System.Threading.Tasks;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IDailyWordRepository
    {
        Task<DailyWordSession?> GetByUserAndListAsync(string userId, string listName, bool trackChanges = false);
        void Create(DailyWordSession session);
        void Update(DailyWordSession session);
        void Delete(DailyWordSession session);
        Task SaveAsync();
    }
}

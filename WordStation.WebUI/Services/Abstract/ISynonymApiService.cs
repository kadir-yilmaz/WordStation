using WordStation.WebUI.Models;

namespace WordStation.WebUI.Services.Abstract
{
    public interface ISynonymApiService
    {
        /// <summary>
        /// Kullanıcının tüm eş anlam gruplarını getirir
        /// </summary>
        Task<IEnumerable<SynonymGroup>> GetAllGroupsAsync(string userId, string token);

        /// <summary>
        /// Belirli bir kelimenin eş anlamlılarını getirir
        /// </summary>
        Task<IEnumerable<Word>> GetSynonymsForWordAsync(int wordId, string userId, string token);

        /// <summary>
        /// Kullanıcıya ait tüm eş anlamlı kelimeleri sözlük formatında getirir
        /// </summary>
        Task<Dictionary<int, IEnumerable<Word>>> GetAllSynonymsForUserAsync(string userId, string token);

        /// <summary>
        /// Yeni eş anlam grubu oluşturur
        /// </summary>
        Task<SynonymGroup?> CreateGroupAsync(string? name, List<int> wordIds, string userId, string token);

        /// <summary>
        /// Gruba kelime ekler
        /// </summary>
        Task<bool> AddWordToGroupAsync(int groupId, int wordId, string userId, string token);

        /// <summary>
        /// Gruptan kelime çıkarır
        /// </summary>
        Task<bool> RemoveWordFromGroupAsync(int groupId, int wordId, string userId, string token);

        /// <summary>
        /// Grubu siler
        /// </summary>
        Task<bool> DeleteGroupAsync(int groupId, string userId, string token);
    }
}

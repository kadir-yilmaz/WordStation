using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface ISynonymGroupService
    {
        /// <summary>
        /// Kullanıcının tüm eş anlam gruplarını getirir
        /// </summary>
        IEnumerable<SynonymGroup> GetAllGroups(string userId);
        
        /// <summary>
        /// ID'ye göre grup getirir
        /// </summary>
        SynonymGroup? GetGroupById(int id, string userId);
        
        /// <summary>
        /// Bir kelimenin eş anlamlılarını getirir (kendisi hariç)
        /// </summary>
        IEnumerable<Word> GetSynonymsForWord(int wordId, string userId);
        
        /// <summary>
        /// Yeni grup oluşturur
        /// </summary>
        SynonymGroup CreateGroup(string? name, List<int> wordIds, string userId);
        
        /// <summary>
        /// Gruba kelime ekler
        /// </summary>
        void AddWordToGroup(int groupId, int wordId, string userId);
        
        /// <summary>
        /// Gruptan kelime çıkarır
        /// </summary>
        void RemoveWordFromGroup(int groupId, int wordId, string userId);
        
        /// <summary>
        /// Grup adını günceller
        /// </summary>
        void UpdateGroupName(int groupId, string? newName, string userId);
        
        /// <summary>
        /// Grubu siler
        /// </summary>
        void DeleteGroup(int groupId, string userId);
    }
}

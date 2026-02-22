using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class SynonymGroupService : ISynonymGroupService
    {
        private readonly ISynonymGroupRepository _repository;

        public SynonymGroupService(ISynonymGroupRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<SynonymGroup> GetAllGroups(string userId)
        {
            return _repository
                .GetByCondition(g => g.UserId == userId, trackChanges: false)
                .ToList();
        }

        public SynonymGroup? GetGroupById(int id, string userId)
        {
            return _repository
                .GetByCondition(g => g.Id == id && g.UserId == userId, trackChanges: false)
                .FirstOrDefault();
        }

        public IEnumerable<Word> GetSynonymsForWord(int wordId, string userId)
        {
            // Kelimenin dahil olduğu tüm grupları bul
            var groups = _repository
                .GetByCondition(g => g.UserId == userId && g.SynonymWords.Any(sw => sw.WordId == wordId), trackChanges: false)
                .ToList();

            // Bu gruplardan kelimeleri topla (kendisi hariç)
            var synonyms = groups
                .SelectMany(g => g.SynonymWords)
                .Where(sw => sw.WordId != wordId)
                .Select(sw => sw.Word)
                .DistinctBy(w => w.Id)
                .ToList();

            return synonyms;
        }

        public Dictionary<int, IEnumerable<Word>> GetAllSynonymsForUser(string userId)
        {
            var groups = _repository
                .GetByCondition(g => g.UserId == userId, trackChanges: false)
                .ToList();

            var result = new Dictionary<int, IEnumerable<Word>>();

            // Get all unique word IDs that are in any synonym group
            var allWordIds = groups
                .SelectMany(g => g.SynonymWords)
                .Select(sw => sw.WordId)
                .Distinct()
                .ToList();
            
            foreach(var wordId in allWordIds)
            {
                var wordGroups = groups.Where(g => g.SynonymWords.Any(sw => sw.WordId == wordId));
                var synonyms = wordGroups
                    .SelectMany(g => g.SynonymWords)
                    .Where(sw => sw.WordId != wordId)
                    .Select(sw => sw.Word)
                    .DistinctBy(w => w.Id)
                    .ToList();
                
                if (synonyms.Any())
                {
                    result[wordId] = synonyms;
                }
            }

            return result;
        }

        public SynonymGroup CreateGroup(string? name, List<int> wordIds, string userId)
        {
            var group = new SynonymGroup
            {
                Name = name,
                UserId = userId,
                SynonymWords = wordIds.Select(wId => new SynonymWord { WordId = wId }).ToList()
            };

            _repository.Create(group);
            _repository.Save();

            return group;
        }

        public void AddWordToGroup(int groupId, int wordId, string userId)
        {
            var group = _repository
                .GetByCondition(g => g.Id == groupId && g.UserId == userId, trackChanges: true)
                .FirstOrDefault();

            if (group == null)
                throw new KeyNotFoundException($"Grup bulunamadı: {groupId}");

            // Zaten ekliyse tekrar ekleme
            if (group.SynonymWords.Any(sw => sw.WordId == wordId))
                return;

            group.SynonymWords.Add(new SynonymWord { WordId = wordId, SynonymGroupId = groupId });
            _repository.Save();
        }

        public void RemoveWordFromGroup(int groupId, int wordId, string userId)
        {
            var group = _repository
                .GetByCondition(g => g.Id == groupId && g.UserId == userId, trackChanges: true)
                .FirstOrDefault();

            if (group == null)
                throw new KeyNotFoundException($"Grup bulunamadı: {groupId}");

            var synonymWord = group.SynonymWords.FirstOrDefault(sw => sw.WordId == wordId);
            if (synonymWord != null)
            {
                group.SynonymWords.Remove(synonymWord);
                _repository.Save();
            }
        }

        public void UpdateGroupName(int groupId, string? newName, string userId)
        {
            var group = _repository
                .GetByCondition(g => g.Id == groupId && g.UserId == userId, trackChanges: true)
                .FirstOrDefault();

            if (group == null)
                throw new KeyNotFoundException($"Grup bulunamadı: {groupId}");

            group.Name = newName;
            _repository.Save();
        }

        public void DeleteGroup(int groupId, string userId)
        {
            var group = _repository
                .GetByCondition(g => g.Id == groupId && g.UserId == userId, trackChanges: true)
                .FirstOrDefault();

            if (group == null)
                throw new KeyNotFoundException($"Grup bulunamadı: {groupId}");

            _repository.Delete(group);
            _repository.Save();
        }
    }
}

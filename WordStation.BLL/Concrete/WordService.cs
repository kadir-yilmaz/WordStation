using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class WordService : IWordService
    {
        private readonly IWordRepository _wordRepository;

        // WordRepository bağımlılığı ile servis başlatılıyor
        public WordService(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public async Task CreateWordAsync(Word word)
        {
            _wordRepository.CreateWord(word);
            await _wordRepository.SaveAsync();
        }

        public async Task DeleteWordAsync(int id)
        {
            var words = await _wordRepository.GetWordsByConditionAsync(w => w.Id == id, trackChanges: true);
            var word = words.FirstOrDefault();
            if (word != null)
            {
                _wordRepository.DeleteWord(word);
                await _wordRepository.SaveAsync();
            }
        }

        public async Task UpdateWordAsync(Word word)
        {
            _wordRepository.UpdateWord(word);
            await _wordRepository.SaveAsync();
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync(string userId, string listName)
        {
            return await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: false);
        }

        public async Task<IEnumerable<Word>> SearchWordAsync(string en, string userId, string listName, string searchMode = "starts")
        {
            var enLower = en.ToLower();
            var listNameLower = listName.ToLower();

            if (searchMode == "contains")
            {
                return await _wordRepository.GetWordsByConditionAsync(
                        w => w.UserId == userId &&
                             w.ListName.ToLower() == listNameLower &&
                             w.En.ToLower().Contains(enLower),
                        trackChanges: false);
            }

            return await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId &&
                         w.ListName.ToLower() == listNameLower &&
                         w.En.ToLower().StartsWith(enLower),
                    trackChanges: false);
        }

        public async Task<IEnumerable<string>> GetListNamesAsync(string userId)
        {
            var words = await _wordRepository.GetWordsByConditionAsync(w => w.UserId == userId, trackChanges: false);
            return words.Select(w => w.ListName)
                        .Distinct()
                        .ToList();
        }

        public async Task UpdateListNameAsync(string listName, string newListName, string userId)
        {
            var words = await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: true);

            foreach (var word in words)
            {
                word.ListName = newListName;
                _wordRepository.UpdateWord(word);
            }
            await _wordRepository.SaveAsync();
        }

        public async Task DeleteListAsync(string listName, string userId)
        {
            var wordsToDelete = await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: true);

            foreach (var word in wordsToDelete)
            {
                _wordRepository.DeleteWord(word);
            }
            await _wordRepository.SaveAsync();
        }
        public async Task<IEnumerable<WordGroupDto>> GetSynonymGroupsAsync(string userId)
        {
            var allWords = await _wordRepository.GetWordsByConditionAsync(w => w.UserId == userId, trackChanges: false);

            return allWords
                .GroupBy(w => w.Tr.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => new WordGroupDto
                {
                    Tr = g.First().Tr, // Original casing of the first word
                    Words = g.ToList()
                })
                .OrderBy(g => g.Tr)
                .ToList();
        }

        public async Task<IEnumerable<Word>> GetAllWordsForUserAsync(string userId)
        {
            return await _wordRepository.GetWordsByConditionAsync(w => w.UserId == userId, trackChanges: false);
        }
    }
}


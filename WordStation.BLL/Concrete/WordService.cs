using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class WordService : IWordService
    {
        private readonly IWordRepository _wordRepository;

        public WordService(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public void CreateWord(Word word)
        {
            _wordRepository.CreateWord(word);
            _wordRepository.Save();
        }

        public void DeleteWord(int id)
        {
            var word = _wordRepository.GetWordsByCondition(w => w.Id == id, trackChanges: true)
                                       .FirstOrDefault();
            if (word != null)
            {
                _wordRepository.DeleteWord(word);
                _wordRepository.Save();
            }
        }

        public void UpdateWord(Word word)
        {
            _wordRepository.UpdateWord(word);
            _wordRepository.Save();
        }

        public IEnumerable<Word> GetAllWords(string userId, string listName)
        {
            return _wordRepository.GetWordsByCondition(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: false)
                .ToList();
        }

        public IEnumerable<Word> SearchWord(string en, string userId, string listName, string searchMode = "starts")
        {
            var enLower = en.ToLower();
            var listNameLower = listName.ToLower();

            if (searchMode == "contains")
            {
                return _wordRepository.GetWordsByCondition(
                        w => w.UserId == userId &&
                             w.ListName.ToLower() == listNameLower &&
                             w.En.ToLower().Contains(enLower),
                        trackChanges: false)
                    .ToList();
            }

            return _wordRepository.GetWordsByCondition(
                    w => w.UserId == userId &&
                         w.ListName.ToLower() == listNameLower &&
                         w.En.ToLower().StartsWith(enLower),
                    trackChanges: false)
                .ToList();
        }

        public IEnumerable<string> GetListNames(string userId)
        {
            return _wordRepository.GetWordsByCondition(w => w.UserId == userId, trackChanges: false)
                           .Select(w => w.ListName)
                           .Distinct()
                           .ToList();
        }

        public void UpdateListName(string listName, string newListName, string userId)
        {
            var words = _wordRepository.GetWordsByCondition(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: true)
                .ToList();

            foreach (var word in words)
            {
                word.ListName = newListName;
                _wordRepository.UpdateWord(word);
            }
            _wordRepository.Save();
        }

        public void DeleteList(string listName, string userId)
        {
            var wordsToDelete = _wordRepository.GetWordsByCondition(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: true)
                .ToList();

            foreach (var word in wordsToDelete)
            {
                _wordRepository.DeleteWord(word);
            }
            _wordRepository.Save();
        }
    }
}

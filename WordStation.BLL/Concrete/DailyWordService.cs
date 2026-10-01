using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class DailyWordService : IDailyWordService
    {
        private readonly IDailyWordRepository _dailyWordRepository;
        private readonly IWordRepository _wordRepository;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DailyWordService(
            IDailyWordRepository dailyWordRepository,
            IWordRepository wordRepository)
        {
            _dailyWordRepository = dailyWordRepository;
            _wordRepository = wordRepository;
        }

        public async Task<DailyWordSessionDto?> GetSessionAsync(string userId, string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return null;

            var session = await _dailyWordRepository.GetByUserAndListAsync(userId, listName);
            if (session == null)
                return null;

            return MapToDto(session);
        }

        public async Task<DailyWordSessionDto> InitializeSessionAsync(string userId, string listName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId zorunludur.", nameof(userId));
            if (string.IsNullOrWhiteSpace(listName))
                throw new ArgumentException("ListName zorunludur.", nameof(listName));

            // Mevcut session varsa sil
            var existing = await _dailyWordRepository.GetByUserAndListAsync(userId, listName, trackChanges: true);
            if (existing != null)
            {
                _dailyWordRepository.Delete(existing);
                await _dailyWordRepository.SaveAsync();
            }

            // Listedeki tüm kelime ID'lerini al
            var words = await _wordRepository.GetWordsByConditionAsync(
                w => w.UserId == userId && w.ListName == listName,
                trackChanges: false);

            var allWordIds = words.Select(w => w.Id).ToList();

            var session = new DailyWordSession
            {
                UserId = userId,
                ListName = listName,
                DailyWordsJson = "[]",
                CompletedWordsJson = "[]",
                RemainingWordIdsJson = JsonSerializer.Serialize(allWordIds),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dailyWordRepository.Create(session);
            await _dailyWordRepository.SaveAsync();

            return MapToDto(session);
        }

        public async Task<DailyWordSessionDto> AddToDailyAsync(AddToDailyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ListName))
                throw new ArgumentException("UserId ve ListName zorunludur.");

            var session = await _dailyWordRepository.GetByUserAndListAsync(dto.UserId, dto.ListName, trackChanges: true);
            if (session == null)
                throw new InvalidOperationException("Aktif session bulunamadı.");

            var dailyWords = DeserializeItems(session.DailyWordsJson);
            var remainingIds = DeserializeIds(session.RemainingWordIdsJson);
            var completedWords = DeserializeItems(session.CompletedWordsJson);

            // Eklenecek kelimeleri DB'den al
            var words = await _wordRepository.GetWordsByConditionAsync(
                w => dto.WordIds.Contains(w.Id), trackChanges: false);

            foreach (var word in words)
            {
                // Zaten günlükte yoksa ekle
                if (!dailyWords.Any(d => d.WordId == word.Id))
                {
                    dailyWords.Add(new DailyWordItemDto
                    {
                        WordId = word.Id,
                        En = word.En,
                        Tr = word.Tr
                    });
                }

                // Remaining'den çıkar
                remainingIds.Remove(word.Id);

                // Completed'dan çıkar (varsa)
                completedWords.RemoveAll(d => d.WordId == word.Id);
            }

            session.DailyWordsJson = JsonSerializer.Serialize(dailyWords, _jsonOptions);
            session.RemainingWordIdsJson = JsonSerializer.Serialize(remainingIds, _jsonOptions);
            session.CompletedWordsJson = JsonSerializer.Serialize(completedWords, _jsonOptions);
            session.UpdatedAt = DateTime.UtcNow;

            _dailyWordRepository.Update(session);
            await _dailyWordRepository.SaveAsync();

            return MapToDto(session);
        }

        public async Task<DailyWordSessionDto> RemoveFromDailyAsync(RemoveFromDailyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ListName))
                throw new ArgumentException("UserId ve ListName zorunludur.");

            var session = await _dailyWordRepository.GetByUserAndListAsync(dto.UserId, dto.ListName, trackChanges: true);
            if (session == null)
                throw new InvalidOperationException("Aktif session bulunamadı.");

            var dailyWords = DeserializeItems(session.DailyWordsJson);
            var remainingIds = DeserializeIds(session.RemainingWordIdsJson);

            foreach (var wordId in dto.WordIds)
            {
                // Günlükten çıkar
                dailyWords.RemoveAll(d => d.WordId == wordId);

                // Remaining'e geri ekle (yoksa)
                if (!remainingIds.Contains(wordId))
                    remainingIds.Add(wordId);
            }

            session.DailyWordsJson = JsonSerializer.Serialize(dailyWords, _jsonOptions);
            session.RemainingWordIdsJson = JsonSerializer.Serialize(remainingIds, _jsonOptions);
            session.UpdatedAt = DateTime.UtcNow;

            _dailyWordRepository.Update(session);
            await _dailyWordRepository.SaveAsync();

            return MapToDto(session);
        }

        public async Task<DailyWordSessionDto> CompleteWordAsync(CompleteDailyWordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ListName))
                throw new ArgumentException("UserId ve ListName zorunludur.");

            var session = await _dailyWordRepository.GetByUserAndListAsync(dto.UserId, dto.ListName, trackChanges: true);
            if (session == null)
                throw new InvalidOperationException("Aktif session bulunamadı.");

            var dailyWords = DeserializeItems(session.DailyWordsJson);
            var completedWords = DeserializeItems(session.CompletedWordsJson);

            var wordToComplete = dailyWords.FirstOrDefault(d => d.WordId == dto.WordId);
            if (wordToComplete != null)
            {
                // Günlükten çıkar
                dailyWords.Remove(wordToComplete);

                // Çalışılmışa ekle
                wordToComplete.CompletedAt = DateTime.UtcNow;
                completedWords.Add(wordToComplete);
            }

            session.DailyWordsJson = JsonSerializer.Serialize(dailyWords, _jsonOptions);
            session.CompletedWordsJson = JsonSerializer.Serialize(completedWords, _jsonOptions);
            session.UpdatedAt = DateTime.UtcNow;

            _dailyWordRepository.Update(session);
            await _dailyWordRepository.SaveAsync();

            return MapToDto(session);
        }

        public async Task<bool> DeleteSessionAsync(string userId, string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return false;

            var session = await _dailyWordRepository.GetByUserAndListAsync(userId, listName, trackChanges: true);
            if (session == null)
                return false;

            _dailyWordRepository.Delete(session);
            await _dailyWordRepository.SaveAsync();
            return true;
        }

        #region Private Helpers

        private static DailyWordSessionDto MapToDto(DailyWordSession session)
        {
            return new DailyWordSessionDto
            {
                Id = session.Id,
                ListName = session.ListName,
                DailyWords = DeserializeItems(session.DailyWordsJson),
                CompletedWords = DeserializeItems(session.CompletedWordsJson),
                RemainingWordIds = DeserializeIds(session.RemainingWordIdsJson),
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }

        private static List<DailyWordItemDto> DeserializeItems(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<DailyWordItemDto>>(json, _jsonOptions) ?? new List<DailyWordItemDto>();
            }
            catch
            {
                return new List<DailyWordItemDto>();
            }
        }

        private static List<int> DeserializeIds(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<int>>(json, _jsonOptions) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        #endregion
    }
}

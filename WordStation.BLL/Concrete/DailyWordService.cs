using System;
using System.Collections.Generic;
using System.Linq;
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

            var session = new DailyWordSession
            {
                UserId = userId,
                ListName = listName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SessionItems = new List<DailyWordSessionItem>()
            };

            foreach (var word in words)
            {
                session.SessionItems.Add(new DailyWordSessionItem
                {
                    WordId = word.Id,
                    Status = 0 // 0: Remaining
                });
            }

            _dailyWordRepository.Create(session);
            await _dailyWordRepository.SaveAsync();

            // Sadece DTO maplemek için DB'den son halini Word'lerle birlikte çek
            return MapToDto(await _dailyWordRepository.GetByUserAndListAsync(userId, listName) ?? session);
        }

        public async Task<DailyWordSessionDto> AddToDailyAsync(AddToDailyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ListName))
                throw new ArgumentException("UserId ve ListName zorunludur.");

            var session = await _dailyWordRepository.GetByUserAndListAsync(dto.UserId, dto.ListName, trackChanges: true);
            if (session == null)
                throw new InvalidOperationException("Aktif session bulunamadı.");

            foreach (var wordId in dto.WordIds)
            {
                var item = session.SessionItems.FirstOrDefault(x => x.WordId == wordId);
                if (item != null)
                {
                    item.Status = 1; // 1: Daily
                }
            }

            session.UpdatedAt = DateTime.UtcNow;

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

            foreach (var wordId in dto.WordIds)
            {
                var item = session.SessionItems.FirstOrDefault(x => x.WordId == wordId);
                if (item != null)
                {
                    item.Status = 0; // 0: Remaining
                }
            }

            session.UpdatedAt = DateTime.UtcNow;

            await _dailyWordRepository.SaveAsync();

            return MapToDto(session);
        }

        public async Task<DailyWordSessionDto> CompleteWordAsync(CompleteDailyWordDto dto)
        {
            return await CompleteWordsAsync(new CompleteDailyWordsDto
            {
                UserId = dto.UserId,
                ListName = dto.ListName,
                WordIds = new List<int> { dto.WordId }
            });
        }

        public async Task<DailyWordSessionDto> CompleteWordsAsync(CompleteDailyWordsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ListName))
                throw new ArgumentException("UserId ve ListName zorunludur.");

            var session = await _dailyWordRepository.GetByUserAndListAsync(dto.UserId, dto.ListName, trackChanges: true);
            if (session == null)
                throw new InvalidOperationException("Aktif session bulunamadı.");

            foreach (var wordId in dto.WordIds)
            {
                var item = session.SessionItems.FirstOrDefault(x => x.WordId == wordId);
                if (item != null)
                {
                    item.Status = 2; // 2: Completed
                    item.CompletedAt = DateTime.UtcNow;
                }
            }

            session.UpdatedAt = DateTime.UtcNow;

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
            var dto = new DailyWordSessionDto
            {
                Id = session.Id,
                ListName = session.ListName,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };

            if (session.SessionItems != null)
            {
                dto.DailyWords = session.SessionItems
                    .Where(x => x.Status == 1 && x.Word != null)
                    .Select(x => new DailyWordItemDto
                    {
                        WordId = x.WordId,
                        En = x.Word.En,
                        Tr = x.Word.Tr,
                        CompletedAt = x.CompletedAt
                    }).ToList();

                dto.CompletedWords = session.SessionItems
                    .Where(x => x.Status == 2 && x.Word != null)
                    .Select(x => new DailyWordItemDto
                    {
                        WordId = x.WordId,
                        En = x.Word.En,
                        Tr = x.Word.Tr,
                        CompletedAt = x.CompletedAt
                    }).ToList();

                dto.RemainingWordIds = session.SessionItems
                    .Where(x => x.Status == 0)
                    .Select(x => x.WordId)
                    .ToList();
            }

            return dto;
        }

        #endregion
    }
}

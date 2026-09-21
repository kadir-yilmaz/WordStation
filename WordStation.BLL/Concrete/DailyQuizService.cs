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
    public class DailyQuizService : IDailyQuizService
    {
        private readonly IDailyQuizRepository _dailyQuizRepository;
        private readonly IWordRepository _wordRepository;

        public DailyQuizService(
            IDailyQuizRepository dailyQuizRepository,
            IWordRepository wordRepository)
        {
            _dailyQuizRepository = dailyQuizRepository;
            _wordRepository = wordRepository;
        }

        public async Task<DailyQuizPlanDto?> GetPlanByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var plan = await _dailyQuizRepository.GetPlanByUserIdAsync(userId, trackChanges: false);
            if (plan == null)
                return null;

            return MapToDto(plan);
        }

        public async Task<DailyQuizPlanDto> CreateOrResetPlanAsync(CreateDailyQuizPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.", nameof(dto.UserId));

            var wordIds = await ResolveWordIdsAsync(dto.UserId, dto.ListName);
            var shuffledJson = JsonSerializer.Serialize(wordIds);

            var existingPlan = await _dailyQuizRepository.GetPlanByUserIdAsync(dto.UserId, trackChanges: true);
            if (existingPlan != null)
            {
                // Deleting old plan cascades and wipes its old DailyPlanDayHistories
                _dailyQuizRepository.DeletePlan(existingPlan);
                await _dailyQuizRepository.SaveAsync();
            }

            var newPlan = new DailyQuizPlan
            {
                UserId = dto.UserId,
                ListName = dto.ListName ?? "Tümü",
                DailyCount = dto.DailyCount > 0 ? dto.DailyCount : 10,
                ShuffledWordIdsJson = shuffledJson,
                CurrentPointer = 0,
                LastCompletedDate = null,
                StreakDays = 0,
                IsEnglishToTurkish = dto.IsEnglishToTurkish,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dailyQuizRepository.CreatePlan(newPlan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(newPlan);
        }

        public async Task<DailyQuizPlanDto?> UpdateProgressAsync(UpdateDailyQuizProgressDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return null;

            var existingPlan = await _dailyQuizRepository.GetPlanByUserIdAsync(dto.UserId, trackChanges: true);
            if (existingPlan == null)
                return null;

            existingPlan.CurrentPointer = dto.NewPointer;
            existingPlan.LastCompletedDate = dto.LastCompletedDate;
            existingPlan.StreakDays = dto.StreakDays;
            existingPlan.UpdatedAt = DateTime.UtcNow;

            _dailyQuizRepository.UpdatePlan(existingPlan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(existingPlan);
        }

        public async Task<bool> DeletePlanAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var existingPlan = await _dailyQuizRepository.GetPlanByUserIdAsync(userId, trackChanges: true);
            if (existingPlan == null)
                return false;

            // Deleting the plan will cascade delete all DailyPlanDayHistories in DB
            _dailyQuizRepository.DeletePlan(existingPlan);
            await _dailyQuizRepository.SaveAsync();

            return true;
        }

        public async Task<List<DailyPlanDayHistoryDto>> GetDayHistoriesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<DailyPlanDayHistoryDto>();

            var plan = await _dailyQuizRepository.GetPlanByUserIdAsync(userId, trackChanges: false);
            if (plan == null)
                return new List<DailyPlanDayHistoryDto>();

            var histories = await _dailyQuizRepository.GetDayHistoriesByPlanIdAsync(plan.Id);
            return histories.Select(h => new DailyPlanDayHistoryDto
            {
                Id = h.Id,
                DailyQuizPlanId = h.DailyQuizPlanId,
                UserId = h.UserId,
                DayNumber = h.DayNumber,
                CompletedAt = h.CompletedAt,
                TotalQuestions = h.TotalQuestions,
                CorrectCount = h.CorrectCount,
                WrongCount = h.WrongCount,
                Score = h.Score,
                MaxScore = h.MaxScore,
                ResultsJson = h.ResultsJson
            }).ToList();
        }

        public async Task<DailyPlanDayHistoryDto?> SaveDayHistoryAsync(string userId, SaveDailyPlanDayDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId) || dto == null)
                return null;

            var plan = await _dailyQuizRepository.GetPlanByUserIdAsync(userId, trackChanges: false);
            if (plan == null)
                return null;

            var entity = new DailyPlanDayHistory
            {
                DailyQuizPlanId = plan.Id,
                UserId = userId,
                DayNumber = dto.DayNumber,
                CompletedAt = DateTime.UtcNow,
                TotalQuestions = dto.TotalQuestions,
                CorrectCount = dto.CorrectCount,
                WrongCount = dto.WrongCount,
                Score = dto.Score,
                MaxScore = dto.MaxScore,
                ResultsJson = dto.ResultsJson ?? "[]"
            };

            _dailyQuizRepository.AddDayHistory(entity);
            await _dailyQuizRepository.SaveAsync();

            return new DailyPlanDayHistoryDto
            {
                Id = entity.Id,
                DailyQuizPlanId = entity.DailyQuizPlanId,
                UserId = entity.UserId,
                DayNumber = entity.DayNumber,
                CompletedAt = entity.CompletedAt,
                TotalQuestions = entity.TotalQuestions,
                CorrectCount = entity.CorrectCount,
                WrongCount = entity.WrongCount,
                Score = entity.Score,
                MaxScore = entity.MaxScore,
                ResultsJson = entity.ResultsJson
            };
        }

        private async Task<List<int>> ResolveWordIdsAsync(string userId, string? listNameInput)
        {
            List<Word> userWords;
            var listName = listNameInput?.Trim() ?? "Tümü";

            if (string.Equals(listName, "Tümü", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(listName, "All", StringComparison.OrdinalIgnoreCase))
            {
                userWords = await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId,
                    trackChanges: false);
            }
            else
            {
                userWords = await _wordRepository.GetWordsByConditionAsync(
                    w => w.UserId == userId && w.ListName == listName,
                    trackChanges: false);
            }

            var random = Random.Shared;
            return userWords
                .Select(w => w.Id)
                .OrderBy(_ => random.Next())
                .ToList();
        }

        private static DailyQuizPlanDto MapToDto(DailyQuizPlan plan)
        {
            List<int> wordIds;
            try
            {
                wordIds = JsonSerializer.Deserialize<List<int>>(plan.ShuffledWordIdsJson) ?? new List<int>();
            }
            catch
            {
                wordIds = new List<int>();
            }

            return new DailyQuizPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                ListName = plan.ListName,
                DailyCount = plan.DailyCount,
                ShuffledWordIds = wordIds,
                CurrentPointer = plan.CurrentPointer,
                LastCompletedDate = plan.LastCompletedDate,
                StreakDays = plan.StreakDays,
                IsEnglishToTurkish = plan.IsEnglishToTurkish,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            };
        }
    }
}

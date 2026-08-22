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

        public DailyQuizService(IDailyQuizRepository dailyQuizRepository, IWordRepository wordRepository)
        {
            _dailyQuizRepository = dailyQuizRepository;
            _wordRepository = wordRepository;
        }

        public async Task<DailyQuizPlanDto?> GetActivePlanByUserIdAsync(string userId)
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

            List<int> wordIds;

            if (dto.ShuffledWordIds != null && dto.ShuffledWordIds.Any())
            {
                wordIds = dto.ShuffledWordIds;
            }
            else
            {
                // Fetch words from database
                List<Word> userWords;
                var listName = dto.ListName?.Trim() ?? "Tümü";

                if (string.Equals(listName, "Tümü", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(listName, "All", StringComparison.OrdinalIgnoreCase))
                {
                    userWords = await _wordRepository.GetWordsByConditionAsync(
                        w => w.UserId == dto.UserId,
                        trackChanges: false);
                }
                else
                {
                    userWords = await _wordRepository.GetWordsByConditionAsync(
                        w => w.UserId == dto.UserId && w.ListName == listName,
                        trackChanges: false);
                }

                // Shuffle words
                var random = Random.Shared;
                wordIds = userWords
                    .Select(w => w.Id)
                    .OrderBy(_ => random.Next())
                    .ToList();
            }

            var shuffledJson = JsonSerializer.Serialize(wordIds);
            var existingPlan = await _dailyQuizRepository.GetPlanByUserIdAsync(dto.UserId, trackChanges: true);

            if (existingPlan != null)
            {
                existingPlan.ListName = dto.ListName ?? "Tümü";
                existingPlan.DailyCount = dto.DailyCount > 0 ? dto.DailyCount : 10;
                existingPlan.ShuffledWordIdsJson = shuffledJson;
                existingPlan.CurrentPointer = 0;
                existingPlan.LastCompletedDate = null;
                existingPlan.StreakDays = 0;
                existingPlan.IsEnglishToTurkish = dto.IsEnglishToTurkish;
                existingPlan.UpdatedAt = DateTime.UtcNow;

                _dailyQuizRepository.UpdatePlan(existingPlan);
                await _dailyQuizRepository.SaveAsync();

                return MapToDto(existingPlan);
            }
            else
            {
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

            _dailyQuizRepository.DeletePlan(existingPlan);
            await _dailyQuizRepository.SaveAsync();
            return true;
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

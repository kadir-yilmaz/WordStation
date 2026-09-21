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

        public async Task<List<DailyQuizPlanDto>> GetAllPlansByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<DailyQuizPlanDto>();

            var plans = await _dailyQuizRepository.GetPlansByUserIdAsync(userId, trackChanges: false);
            return plans.Select(MapToDto).ToList();
        }

        public async Task<DailyQuizPlanDto?> GetPlanByIdAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0)
                return null;

            var plan = await _dailyQuizRepository.GetPlanByIdAsync(planId, trackChanges: false);
            if (plan == null || plan.UserId != userId)
                return null;

            return MapToDto(plan);
        }

        public async Task<DailyQuizPlanDto?> GetActivePlanByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var plan = await _dailyQuizRepository.GetActivePlanByUserIdAsync(userId, trackChanges: false);
            if (plan == null)
                return null;

            return MapToDto(plan);
        }

        public async Task<DailyQuizPlanDto> CreatePlanAsync(CreateDailyQuizPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.", nameof(dto.UserId));

            var wordIds = await ResolveWordIdsAsync(dto.UserId, dto.ListName, dto.ShuffledWordIds);
            var shuffledJson = JsonSerializer.Serialize(wordIds);

            // Handle IsActive logic
            if (dto.SetAsActive)
            {
                var existingPlans = await _dailyQuizRepository.GetPlansByUserIdAsync(dto.UserId, trackChanges: true);
                foreach (var p in existingPlans.Where(p => p.IsActive))
                {
                    p.IsActive = false;
                    _dailyQuizRepository.UpdatePlan(p);
                }
            }

            var title = !string.IsNullOrWhiteSpace(dto.Title)
                ? dto.Title.Trim()
                : $"{dto.ListName ?? "Tümü"} {(dto.PlanType == PlanType.OpenBuffet ? "Açık Büfe" : "Sıralı Plan")}";

            var newPlan = new DailyQuizPlan
            {
                UserId = dto.UserId,
                Title = title,
                ListName = dto.ListName ?? "Tümü",
                PlanType = dto.PlanType,
                DailyCount = dto.DailyCount > 0 ? dto.DailyCount : 10,
                ShuffledWordIdsJson = shuffledJson,
                CompletedWordIdsJson = "[]",
                DailySelectedWordIdsJson = "[]",
                CurrentPointer = 0,
                LastCompletedDate = null,
                StreakDays = 0,
                IsEnglishToTurkish = dto.IsEnglishToTurkish,
                IsActive = dto.SetAsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dailyQuizRepository.CreatePlan(newPlan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(newPlan);
        }

        public async Task<DailyQuizPlanDto> CreateOrResetPlanAsync(CreateDailyQuizPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.", nameof(dto.UserId));

            var wordIds = await ResolveWordIdsAsync(dto.UserId, dto.ListName, dto.ShuffledWordIds);
            var shuffledJson = JsonSerializer.Serialize(wordIds);

            var existingPlan = await _dailyQuizRepository.GetActivePlanByUserIdAsync(dto.UserId, trackChanges: true);

            if (existingPlan != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    existingPlan.Title = dto.Title.Trim();
                existingPlan.ListName = dto.ListName ?? "Tümü";
                existingPlan.PlanType = dto.PlanType;
                existingPlan.DailyCount = dto.DailyCount > 0 ? dto.DailyCount : 10;
                existingPlan.ShuffledWordIdsJson = shuffledJson;
                existingPlan.CompletedWordIdsJson = "[]";
                existingPlan.DailySelectedWordIdsJson = "[]";
                existingPlan.CurrentPointer = 0;
                existingPlan.LastCompletedDate = null;
                existingPlan.StreakDays = 0;
                existingPlan.IsEnglishToTurkish = dto.IsEnglishToTurkish;
                existingPlan.IsActive = true;
                existingPlan.UpdatedAt = DateTime.UtcNow;

                _dailyQuizRepository.UpdatePlan(existingPlan);
                await _dailyQuizRepository.SaveAsync();

                return MapToDto(existingPlan);
            }
            else
            {
                return await CreatePlanAsync(dto);
            }
        }

        public async Task<bool> SetActivePlanAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0)
                return false;

            var userPlans = await _dailyQuizRepository.GetPlansByUserIdAsync(userId, trackChanges: true);
            var target = userPlans.FirstOrDefault(p => p.Id == planId);
            if (target == null)
                return false;

            foreach (var p in userPlans)
            {
                p.IsActive = (p.Id == planId);
                _dailyQuizRepository.UpdatePlan(p);
            }

            await _dailyQuizRepository.SaveAsync();
            return true;
        }

        public async Task<DailyQuizPlanDto?> SelectBuffetWordsAsync(int planId, string userId, SelectBuffetWordsDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0)
                return null;

            var plan = await _dailyQuizRepository.GetPlanByIdAsync(planId, trackChanges: true);
            if (plan == null || plan.UserId != userId)
                return null;

            var selectedIds = dto.SelectedWordIds ?? new List<int>();
            plan.DailySelectedWordIdsJson = JsonSerializer.Serialize(selectedIds);
            plan.UpdatedAt = DateTime.UtcNow;

            _dailyQuizRepository.UpdatePlan(plan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(plan);
        }

        public async Task<DailyQuizPlanDto?> ReturnWordToBuffetPoolAsync(int planId, string userId, int wordId)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0 || wordId <= 0)
                return null;

            var plan = await _dailyQuizRepository.GetPlanByIdAsync(planId, trackChanges: true);
            if (plan == null || plan.UserId != userId)
                return null;

            List<int> completed;
            try
            {
                completed = JsonSerializer.Deserialize<List<int>>(plan.CompletedWordIdsJson) ?? new List<int>();
            }
            catch
            {
                completed = new List<int>();
            }

            if (completed.Remove(wordId))
            {
                plan.CompletedWordIdsJson = JsonSerializer.Serialize(completed);
                plan.UpdatedAt = DateTime.UtcNow;
                _dailyQuizRepository.UpdatePlan(plan);
                await _dailyQuizRepository.SaveAsync();
            }

            return MapToDto(plan);
        }

        public async Task<DailyQuizPlanDto?> UpdateProgressAsync(UpdateDailyQuizProgressDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return null;

            DailyQuizPlan? existingPlan;
            if (dto.PlanId.HasValue && dto.PlanId.Value > 0)
            {
                existingPlan = await _dailyQuizRepository.GetPlanByIdAsync(dto.PlanId.Value, trackChanges: true);
                if (existingPlan != null && existingPlan.UserId != dto.UserId)
                    return null;
            }
            else
            {
                existingPlan = await _dailyQuizRepository.GetActivePlanByUserIdAsync(dto.UserId, trackChanges: true);
            }

            if (existingPlan == null)
                return null;

            if (existingPlan.PlanType == PlanType.OpenBuffet)
            {
                // In OpenBuffet mode, move daily selected words (or passed completed words) to completed pool
                List<int> completedIds;
                try
                {
                    completedIds = JsonSerializer.Deserialize<List<int>>(existingPlan.CompletedWordIdsJson) ?? new List<int>();
                }
                catch
                {
                    completedIds = new List<int>();
                }

                IEnumerable<int> newlyCompleted;
                if (dto.CompletedWordIds != null && dto.CompletedWordIds.Any())
                {
                    newlyCompleted = dto.CompletedWordIds;
                }
                else
                {
                    try
                    {
                        newlyCompleted = JsonSerializer.Deserialize<List<int>>(existingPlan.DailySelectedWordIdsJson) ?? new List<int>();
                    }
                    catch
                    {
                        newlyCompleted = new List<int>();
                    }
                }

                var completedSet = new HashSet<int>(completedIds);
                foreach (var id in newlyCompleted)
                {
                    completedSet.Add(id);
                }

                existingPlan.CompletedWordIdsJson = JsonSerializer.Serialize(completedSet.ToList());
                existingPlan.LastCompletedDate = dto.LastCompletedDate;
                existingPlan.StreakDays = dto.StreakDays;
                existingPlan.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                existingPlan.CurrentPointer = dto.NewPointer;
                existingPlan.LastCompletedDate = dto.LastCompletedDate;
                existingPlan.StreakDays = dto.StreakDays;
                existingPlan.UpdatedAt = DateTime.UtcNow;
            }

            _dailyQuizRepository.UpdatePlan(existingPlan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(existingPlan);
        }

        public async Task<DailyQuizPlanDto?> ResetPlanProgressAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0)
                return null;

            var plan = await _dailyQuizRepository.GetPlanByIdAsync(planId, trackChanges: true);
            if (plan == null || plan.UserId != userId)
                return null;

            plan.CurrentPointer = 0;
            plan.CompletedWordIdsJson = "[]";
            plan.DailySelectedWordIdsJson = "[]";
            plan.LastCompletedDate = null;
            plan.StreakDays = 0;
            plan.UpdatedAt = DateTime.UtcNow;

            _dailyQuizRepository.UpdatePlan(plan);
            await _dailyQuizRepository.SaveAsync();

            return MapToDto(plan);
        }

        public async Task<bool> DeletePlanAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || planId <= 0)
                return false;

            var existingPlan = await _dailyQuizRepository.GetPlanByIdAsync(planId, trackChanges: true);
            if (existingPlan == null || existingPlan.UserId != userId)
                return false;

            var wasActive = existingPlan.IsActive;

            _dailyQuizRepository.DeletePlan(existingPlan);
            await _dailyQuizRepository.SaveAsync();

            if (wasActive)
            {
                var remainingPlans = await _dailyQuizRepository.GetPlansByUserIdAsync(userId, trackChanges: true);
                var nextActive = remainingPlans.FirstOrDefault();
                if (nextActive != null)
                {
                    nextActive.IsActive = true;
                    _dailyQuizRepository.UpdatePlan(nextActive);
                    await _dailyQuizRepository.SaveAsync();
                }
            }

            return true;
        }

        public async Task<bool> DeletePlanAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var existingPlan = await _dailyQuizRepository.GetActivePlanByUserIdAsync(userId, trackChanges: true);
            if (existingPlan == null)
                return false;

            return await DeletePlanAsync(existingPlan.Id, userId);
        }

        private async Task<List<int>> ResolveWordIdsAsync(string userId, string? listNameInput, List<int>? customIds)
        {
            if (customIds != null && customIds.Any())
                return customIds;

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

            List<int> completedIds;
            try
            {
                completedIds = JsonSerializer.Deserialize<List<int>>(plan.CompletedWordIdsJson) ?? new List<int>();
            }
            catch
            {
                completedIds = new List<int>();
            }

            List<int> dailySelectedIds;
            try
            {
                dailySelectedIds = JsonSerializer.Deserialize<List<int>>(plan.DailySelectedWordIdsJson) ?? new List<int>();
            }
            catch
            {
                dailySelectedIds = new List<int>();
            }

            return new DailyQuizPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                Title = string.IsNullOrWhiteSpace(plan.Title)
                    ? $"{plan.ListName} {(plan.PlanType == PlanType.OpenBuffet ? "Açık Büfe" : "Sıralı Plan")}"
                    : plan.Title,
                ListName = plan.ListName,
                PlanType = plan.PlanType,
                DailyCount = plan.DailyCount,
                ShuffledWordIds = wordIds,
                CompletedWordIds = completedIds,
                DailySelectedWordIds = dailySelectedIds,
                CurrentPointer = plan.CurrentPointer,
                LastCompletedDate = plan.LastCompletedDate,
                StreakDays = plan.StreakDays,
                IsEnglishToTurkish = plan.IsEnglishToTurkish,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            };
        }
    }
}

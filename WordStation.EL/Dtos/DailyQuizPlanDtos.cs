using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WordStation.EL.Models;

namespace WordStation.EL.Dtos
{
    public class DailyQuizPlanDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ListName { get; set; } = "Tümü";
        public PlanType PlanType { get; set; } = PlanType.Sequential;
        public int DailyCount { get; set; } = 10;
        public List<int> ShuffledWordIds { get; set; } = new();
        public List<int> CompletedWordIds { get; set; } = new();
        public List<int> DailySelectedWordIds { get; set; } = new();
        public int CurrentPointer { get; set; } = 0;
        public string? LastCompletedDate { get; set; }
        public int StreakDays { get; set; } = 0;
        public bool IsEnglishToTurkish { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int TotalWordsCount => ShuffledWordIds?.Count ?? 0;
        public int CompletedWordsCount => PlanType == PlanType.OpenBuffet
            ? (CompletedWordIds?.Count ?? 0)
            : Math.Min(CurrentPointer, TotalWordsCount);
        public int RemainingWordsCount => Math.Max(0, TotalWordsCount - CompletedWordsCount);
        public int DailySelectedCount => DailySelectedWordIds?.Count ?? 0;
    }

    public class CreateDailyQuizPlanDto
    {
        public string? UserId { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [Required]
        public string ListName { get; set; } = "Tümü";

        public PlanType PlanType { get; set; } = PlanType.Sequential;

        [Range(1, 500)]
        public int DailyCount { get; set; } = 10;

        public bool IsEnglishToTurkish { get; set; } = true;

        public List<int>? ShuffledWordIds { get; set; }

        public bool SetAsActive { get; set; } = true;
    }

    public class SelectBuffetWordsDto
    {
        public string? UserId { get; set; }

        [Required]
        public List<int> SelectedWordIds { get; set; } = new();
    }

    public class UpdateDailyQuizProgressDto
    {
        public string? UserId { get; set; }

        public int? PlanId { get; set; }

        [Range(0, int.MaxValue)]
        public int NewPointer { get; set; }

        public List<int>? CompletedWordIds { get; set; }

        [Required]
        public string LastCompletedDate { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int StreakDays { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Dtos
{
    public class DailyQuizPlanDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = "Tümü";
        public int DailyCount { get; set; } = 10;
        public List<int> ShuffledWordIds { get; set; } = new();
        public int CurrentPointer { get; set; } = 0;
        public string? LastCompletedDate { get; set; }
        public int StreakDays { get; set; } = 0;
        public bool IsEnglishToTurkish { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDailyQuizPlanDto
    {
        public string? UserId { get; set; }

        [Required]
        public string ListName { get; set; } = "Tümü";

        [Range(1, 500)]
        public int DailyCount { get; set; } = 10;

        public bool IsEnglishToTurkish { get; set; } = true;

        public List<int>? ShuffledWordIds { get; set; }
    }

    public class UpdateDailyQuizProgressDto
    {
        public string? UserId { get; set; }

        [Range(0, int.MaxValue)]
        public int NewPointer { get; set; }

        [Required]
        public string LastCompletedDate { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int StreakDays { get; set; }
    }
}

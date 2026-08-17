using System;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Models
{
    public class DailyQuizPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ListName { get; set; } = "Tümü";

        public int DailyCount { get; set; } = 10;

        /// <summary>
        /// Karıştırılmış kelime ID dizisi JSON formatında saklanır. Örn: [102, 55, 1, 990]
        /// </summary>
        [Required]
        public string ShuffledWordIdsJson { get; set; } = "[]";

        public int CurrentPointer { get; set; } = 0;

        [MaxLength(20)]
        public string? LastCompletedDate { get; set; } // YYYY-MM-DD

        public int StreakDays { get; set; } = 0;

        public bool IsEnglishToTurkish { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

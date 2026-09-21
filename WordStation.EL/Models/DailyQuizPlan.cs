using System;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Models
{
    public enum PlanType
    {
        Sequential = 0, // Otomatik Sıralı (Sıfır Tekrar)
        OpenBuffet = 1  // Açık Büfe (Manuel Günlük Kelime Seçimi)
    }

    public class DailyQuizPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ListName { get; set; } = "Tümü";

        public PlanType PlanType { get; set; } = PlanType.Sequential;

        public int DailyCount { get; set; } = 10;

        /// <summary>
        /// Karıştırılmış kelime ID dizisi JSON formatında saklanır. Örn: [102, 55, 1, 990]
        /// </summary>
        [Required]
        public string ShuffledWordIdsJson { get; set; } = "[]";

        /// <summary>
        /// Açık büfe veya sıralı planda havuzdan düşen (tamamlanmış) kelime ID dizisi: [1, 5, 22...]
        /// </summary>
        [Required]
        public string CompletedWordIdsJson { get; set; } = "[]";

        /// <summary>
        /// Açık büfe modunda bugün için seçilmiş kelime ID dizisi: [10, 42, 88...]
        /// </summary>
        [Required]
        public string DailySelectedWordIdsJson { get; set; } = "[]";

        public int CurrentPointer { get; set; } = 0;

        [MaxLength(20)]
        public string? LastCompletedDate { get; set; } // YYYY-MM-DD

        public int StreakDays { get; set; } = 0;

        public bool IsEnglishToTurkish { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

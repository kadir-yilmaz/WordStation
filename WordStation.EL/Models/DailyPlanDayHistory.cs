using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordStation.EL.Models
{
    public class DailyPlanDayHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DailyQuizPlanId { get; set; }

        [ForeignKey(nameof(DailyQuizPlanId))]
        public DailyQuizPlan? DailyQuizPlan { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int DayNumber { get; set; } // 1, 2, 3...

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public int TotalQuestions { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public int Score { get; set; }

        public int MaxScore { get; set; }

        /// <summary>
        /// Soru ve cevap detayları JSON formatında saklanır.
        /// </summary>
        [Required]
        public string ResultsJson { get; set; } = "[]";
    }
}

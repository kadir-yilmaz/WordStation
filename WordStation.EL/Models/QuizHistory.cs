using System;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Models
{
    public class QuizHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "Genel Test";

        public int Score { get; set; }

        public int MaxScore { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public bool IsDailyQuiz { get; set; }

        /// <summary>
        /// Soru ve cevap detayları JSON formatında saklanır.
        /// </summary>
        [Required]
        public string ResultsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

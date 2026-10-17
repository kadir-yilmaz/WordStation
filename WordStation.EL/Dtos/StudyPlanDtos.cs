using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Dtos
{
    // ══════════════════════════════════════════════════════════════
    // Request DTOs
    // ══════════════════════════════════════════════════════════════

    public class CreateStudyPlanDto
    {
        public string? UserId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [Required]
        public List<string> ListNames { get; set; } = new();

        [Range(1, 100)]
        public int DailyWordLimit { get; set; } = 20;
    }

    public class SubmitPlanQuizDto
    {
        public string? UserId { get; set; }

        public int StudyPlanId { get; set; }

        public int DayNumber { get; set; }

        [Required]
        public List<PlanQuizAnswerDto> Answers { get; set; } = new();
    }

    public class PlanQuizAnswerDto
    {
        public int WordId { get; set; }

        public bool IsCorrect { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Response DTOs
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Plan listesi görünümünde kullanılan hafif DTO.
    /// </summary>
    public class StudyPlanSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DailyWordLimit { get; set; }
        public int TotalDays { get; set; }
        public int CompletedDays { get; set; }
        public int TotalWords { get; set; }
        public bool IsActive { get; set; }
        public double Progress { get; set; }
        public List<string> IncludedListNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Plan detay ve tüm günleri içeren DTO.
    /// </summary>
    public class StudyPlanDetailDto : StudyPlanSummaryDto
    {
        public List<StudyPlanDayDto> Days { get; set; } = new();
    }

    /// <summary>
    /// Belirli bir günün detay DTO'su.
    /// </summary>
    public class StudyPlanDayDto
    {
        public int Id { get; set; }
        public int DayNumber { get; set; }
        public bool IsCompleted { get; set; }
        public int? QuizScore { get; set; }
        public int? QuizMaxScore { get; set; }
        public int? CorrectCount { get; set; }
        public int? WrongCount { get; set; }
        public List<StudyPlanWordDto> Words { get; set; } = new();
    }

    /// <summary>
    /// Kelime bilgisi DTO'su.
    /// </summary>
    public class StudyPlanWordDto
    {
        public int WordId { get; set; }
        public string En { get; set; } = string.Empty;
        public string Tr { get; set; } = string.Empty;
        public string? Example { get; set; }
        public bool IsCorrectInQuiz { get; set; }
    }

    /// <summary>
    /// Quiz soruları response DTO'su.
    /// </summary>
    public class StudyPlanQuizDto
    {
        public int DayNumber { get; set; }
        public List<StudyPlanQuizQuestionDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// Tek bir quiz sorusu.
    /// </summary>
    public class StudyPlanQuizQuestionDto
    {
        public int WordId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordStation.EL.Models
{
    public class StudyPlanDay
    {
        [Key]
        public int Id { get; set; }

        public int StudyPlanId { get; set; }

        [ForeignKey("StudyPlanId")]
        public StudyPlan StudyPlan { get; set; } = null!;

        public int DayNumber { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public int? QuizScore { get; set; }

        public int? QuizMaxScore { get; set; }

        public int? CorrectCount { get; set; }

        public int? WrongCount { get; set; }

        // Navigation
        public ICollection<StudyPlanDayWord> Words { get; set; } = new List<StudyPlanDayWord>();
    }
}

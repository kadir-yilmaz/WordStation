using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Models
{
    public class StudyPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int DailyWordLimit { get; set; } = 20;

        public int TotalDays { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<StudyPlanList> IncludedLists { get; set; } = new List<StudyPlanList>();
        public ICollection<StudyPlanDay> Days { get; set; } = new List<StudyPlanDay>();
    }
}

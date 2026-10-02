using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Models
{
    public class DailyWordSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ListName { get; set; } = string.Empty;

        public ICollection<DailyWordSessionItem> SessionItems { get; set; } = new List<DailyWordSessionItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

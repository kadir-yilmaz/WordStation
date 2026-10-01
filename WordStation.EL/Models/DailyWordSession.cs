using System;
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

        /// <summary>
        /// Günlük çalışma listesi (sağ panel) - JSON: [{ "wordId": 1, "en": "blue", "tr": "mavi" }]
        /// </summary>
        [Required]
        public string DailyWordsJson { get; set; } = "[]";

        /// <summary>
        /// Çalışılmış (tamamlanmış) kelimeler - JSON: [{ "wordId": 1, "en": "blue", "tr": "mavi", "completedAt": "..." }]
        /// </summary>
        [Required]
        public string CompletedWordsJson { get; set; } = "[]";

        /// <summary>
        /// Kalan kelimeler (henüz günlüğe eklenmemiş) - JSON: [1, 2, 3, ...]
        /// </summary>
        [Required]
        public string RemainingWordIdsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordStation.EL.Models
{
    public class DailyWordSessionItem
    {
        [Key]
        public int Id { get; set; }

        public int SessionId { get; set; }
        
        [ForeignKey("SessionId")]
        public DailyWordSession Session { get; set; }

        public int WordId { get; set; }
        
        [ForeignKey("WordId")]
        public Word Word { get; set; }

        /// <summary>
        /// 0 = Remaining (Kalanlar)
        /// 1 = Daily (Çalışılanlar/Günlüğe Alınanlar)
        /// 2 = Completed (Tamamlananlar)
        /// </summary>
        public byte Status { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}

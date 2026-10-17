using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordStation.EL.Models
{
    public class StudyPlanDayWord
    {
        [Key]
        public int Id { get; set; }

        public int StudyPlanDayId { get; set; }

        [ForeignKey("StudyPlanDayId")]
        public StudyPlanDay Day { get; set; } = null!;

        public int WordId { get; set; }

        [ForeignKey("WordId")]
        public Word Word { get; set; } = null!;

        public bool IsCorrectInQuiz { get; set; } = false;
    }
}

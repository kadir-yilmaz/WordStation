using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordStation.EL.Models
{
    public class StudyPlanList
    {
        [Key]
        public int Id { get; set; }

        public int StudyPlanId { get; set; }

        [ForeignKey("StudyPlanId")]
        public StudyPlan StudyPlan { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string ListName { get; set; } = string.Empty;
    }
}

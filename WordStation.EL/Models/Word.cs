using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordStation.EL.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string En { get; set; } = string.Empty;
        public string Tr { get; set; } = string.Empty;
        public string? Example { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
        
    }
}

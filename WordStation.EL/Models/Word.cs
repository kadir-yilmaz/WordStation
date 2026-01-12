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
        public string En { get; set; }
        public string Tr { get; set; }
        public string? Example { get; set; }
        public string UserId { get; set; }
        public string ListName { get; set; }
    }
}

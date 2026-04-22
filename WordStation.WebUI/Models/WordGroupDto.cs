using System.Collections.Generic;

namespace WordStation.WebUI.Models
{
    public class WordGroupDto
    {
        public string Tr { get; set; } = string.Empty;
        public List<Word> Words { get; set; } = new();
    }
}

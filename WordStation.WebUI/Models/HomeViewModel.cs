using WordStation.WebUI.Models; 

namespace WordStation.WebUI.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Word> Words { get; set; } = new List<Word>();
        public IEnumerable<string> AllLists { get; set; } = new List<string>();
        public IEnumerable<Word> AllWords { get; set; } = new List<Word>();
        public IEnumerable<WordGroupDto> SynonymGroups { get; set; } = new List<WordGroupDto>();
        
        public string SelectedList { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchMode { get; set; } = "starts";
    }
}
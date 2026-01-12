using WordStation.WebUI.Models; 

namespace WordStation.WebUI.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Word> Words { get; set; }
        public IEnumerable<string> ListNames { get; set; }
        public string SelectedList { get; set; }
        public string SearchTerm { get; set; }
    }
}
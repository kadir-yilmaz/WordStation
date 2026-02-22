namespace WordStation.WebAPI.Models
{
    public class HomeViewModel
    {
        public List<string> ListNames { get; set; } = new();
        public List<WordDto> Words { get; set; } = new();
        public string SelectedList { get; set; }
        public string Search { get; set; }
    }
}

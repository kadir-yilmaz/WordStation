using System.Collections.Generic;

namespace WordStation.WebUI.Models
{
    public class CompletedWordViewModel
    {
        public int Id { get; set; }
        public string En { get; set; } = string.Empty;
        public string Tr { get; set; } = string.Empty;
        public string Example { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class DailyWordViewModel
    {
        // Liste seçim sayfası
        public List<string> AllLists { get; set; } = new();
        public Dictionary<string, int> ListWordCounts { get; set; } = new();
        public Dictionary<string, int> ListCompletedCounts { get; set; } = new();

        // Kelime çalışma sayfası
        public string SelectedList { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;

        // Sol panel verileri
        public List<Word> AllWords { get; set; } = new();             // Tüm kelimeler
        public List<CompletedWordViewModel> CompletedWords { get; set; } = new(); // Çalışılmış kelimeler
        public int TotalWordCount { get; set; }
        public int CompletedWordCount { get; set; }

        // Sağ panel: Günlük kelimeler
        public List<Word> DailyWords { get; set; } = new();

        // Aktif tab
        public string ActiveTab { get; set; } = "all"; // "all" | "completed"

        // Session durumu
        public bool HasSession { get; set; }
    }
}

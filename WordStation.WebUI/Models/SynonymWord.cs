namespace WordStation.WebUI.Models
{
    /// <summary>
    /// Bağlantı modeli: Hangi kelime hangi gruba dahil
    /// </summary>
    public class SynonymWord
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public int SynonymGroupId { get; set; }
        
        // Navigation (API'den gelirse)
        public Word? Word { get; set; }
        public SynonymGroup? SynonymGroup { get; set; }
    }
}

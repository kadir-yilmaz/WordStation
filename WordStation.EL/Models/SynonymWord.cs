namespace WordStation.EL.Models
{
    /// <summary>
    /// Bağlantı tablosu: Hangi kelime hangi gruba dahil?
    /// </summary>
    public class SynonymWord
    {
        public int Id { get; set; }
        
        public int WordId { get; set; }
        public Word Word { get; set; } = null!;
        
        public int SynonymGroupId { get; set; }
        public SynonymGroup SynonymGroup { get; set; } = null!;
    }
}

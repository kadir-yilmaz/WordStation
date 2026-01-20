namespace WordStation.EL.Models
{
    public class SynonymGroup
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Opsiyonel: "Reddetme Grubu" gibi açıklayıcı isim
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Grubun sahibi (kullanıcıya özel gruplar)
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Bu gruptaki kelimeler
        /// </summary>
        public List<SynonymWord> SynonymWords { get; set; } = new();
    }
}

namespace WordStation.WebUI.Models
{
    /// <summary>
    /// Eş anlam grubu - birden fazla kelimenin eş anlamlı olduğunu gösterir
    /// </summary>
    public class SynonymGroup
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<SynonymWord> SynonymWords { get; set; } = new();
    }
}

namespace WordStation.WebAPI.Models
{
    /// <summary>
    /// PATCH operasyonu için kullanılan DTO.
    /// Sadece gönderilen alanlar güncellenir, null olanlar dokunulmaz.
    /// </summary>
    public class WordDtoForPatch
    {
        public string? En { get; set; }
        public string? Tr { get; set; }
        public string? Example { get; set; }
    }
}

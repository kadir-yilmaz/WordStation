using WordStation.WebUI.Models;

namespace WordStation.WebUI.ViewModels
{
    public class SynonymViewModel
    {
        /// <summary>Aynı Türkçe anlama sahip en az 2 kelimeden oluşan gruplar.</summary>
        public List<WordGroupDto> Groups { get; set; } = new();

        /// <summary>
        /// Hiçbir eş anlamlı gruba dahil olmayan kelimeler (Türkçe anlama göre gruplanmış).
        /// Sadece arama yapıldığında, eşleşenler "Tekil Kelimeler" bölümünde gösterilir.
        /// </summary>
        public List<WordGroupDto> SingleWords { get; set; } = new();
    }
}


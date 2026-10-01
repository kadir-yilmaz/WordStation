using System;
using System.Collections.Generic;

namespace WordStation.EL.Dtos
{
    // ===== Response DTO =====
    public class DailyWordSessionDto
    {
        public int Id { get; set; }
        public string ListName { get; set; } = string.Empty;
        public List<DailyWordItemDto> DailyWords { get; set; } = new();
        public List<DailyWordItemDto> CompletedWords { get; set; } = new();
        public List<int> RemainingWordIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DailyWordItemDto
    {
        public int WordId { get; set; }
        public string En { get; set; } = string.Empty;
        public string Tr { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }

    // ===== Request DTO: Kelime ekleme =====
    public class AddToDailyDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
        public List<int> WordIds { get; set; } = new();
    }

    // ===== Request DTO: Kelime çıkarma =====
    public class RemoveFromDailyDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
        public List<int> WordIds { get; set; } = new();
    }

    // ===== Request DTO: Kelime tamamlama =====
    public class CompleteDailyWordDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
        public int WordId { get; set; }
    }

    // ===== Request DTO: Session başlatma =====
    public class InitDailyWordSessionDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;
    }
}

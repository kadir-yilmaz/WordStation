using System.ComponentModel.DataAnnotations;

namespace WordStation.EL.Dtos
{
    public record GoogleLoginDto
    {
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; init; } = string.Empty;

        public string? GoogleId { get; init; }
        public string? Name { get; init; }
    }
}

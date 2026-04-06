using System.ComponentModel.DataAnnotations;

namespace WordStation.WebUI.Models
{
    public class LoginModel
    {
        private string? _returnurl;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string? Password { get; set; }

        public string ReturnUrl
        {
            get
            {
                if (_returnurl is null)
                    return "/";
                else
                    return _returnurl;
            }
            set
            {
                _returnurl = value;
            }
        }
    }
}

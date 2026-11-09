using System;
using WordStation.BLL.Abstract;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete.Policies
{
    /// <summary>
    /// Tarayıcı ve MVC istemcileri için oturum politikası.
    /// Sabit 7 günlük süre uygular, refresh sırasında süre uzatılmaz (Fixed Expiration).
    /// 1 hafta sonra zorunlu olarak yeniden giriş istenir.
    /// </summary>
    public class WebSessionPolicy : IClientSessionPolicy
    {
        public string ClientType => "web";

        public DateTime CalculateInitialRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(7);
        }

        public DateTime CalculateRefreshedTokenExpiry(RefreshToken currentToken)
        {
            // Sabit süre korunur, sliding yapılmaz.
            return currentToken.Expires;
        }
    }
}


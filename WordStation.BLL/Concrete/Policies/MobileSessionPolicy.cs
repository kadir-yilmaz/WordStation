using System;
using WordStation.BLL.Abstract;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete.Policies
{
    /// <summary>
    /// Mobil ve masaüstü (Flutter vb.) istemciler için oturum politikası (Instagram modeli).
    /// Sliding Expiration uygular: Kullanıcı uygulamayı her kullandığında (refresh sırasında)
    /// oturum süresi otomatik olarak +30 gün ileri atılır. Kullanıcı aktif kaldığı sürece
    /// ve elle çıkış yapmadığı sürece asla yeniden login istemez.
    /// </summary>
    public class MobileSessionPolicy : IClientSessionPolicy
    {
        public string ClientType => "mobile";
        private const int SessionDurationDays = 30;

        public DateTime CalculateInitialRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(SessionDurationDays);
        }

        public DateTime CalculateRefreshedTokenExpiry(RefreshToken currentToken)
        {
            // Sliding Expiration: Her refresh'te süre baştan başlar
            return DateTime.UtcNow.AddDays(SessionDurationDays);
        }
    }
}


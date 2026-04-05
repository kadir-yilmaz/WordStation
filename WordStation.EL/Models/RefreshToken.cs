using System;

namespace WordStation.EL.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } // IdentityUser Id
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Revoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}

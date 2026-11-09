using System;
using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface IClientSessionPolicy
    {
        string ClientType { get; }
        DateTime CalculateInitialRefreshTokenExpiry();
        DateTime CalculateRefreshedTokenExpiry(RefreshToken currentToken);
    }
}


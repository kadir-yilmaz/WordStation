using System;
using System.Collections.Generic;
using System.Linq;
using WordStation.BLL.Abstract;

namespace WordStation.BLL.Concrete.Policies
{
    public class ClientSessionPolicyResolver : IClientSessionPolicyResolver
    {
        private readonly IEnumerable<IClientSessionPolicy> _policies;

        public ClientSessionPolicyResolver(IEnumerable<IClientSessionPolicy> policies)
        {
            _policies = policies;
        }

        public IClientSessionPolicy Resolve(string? clientType = null)
        {
            var normalizedType = clientType?.Trim().ToLowerInvariant();

            // "mobile", "flutter", "ios", "android", "desktop" gibi tipler mobile policy ile eşleşir
            if (normalizedType == "mobile" || normalizedType == "flutter" || 
                normalizedType == "android" || normalizedType == "ios" || 
                normalizedType == "desktop")
            {
                var mobilePolicy = _policies.FirstOrDefault(p => p.ClientType.Equals("mobile", StringComparison.OrdinalIgnoreCase));
                if (mobilePolicy != null) return mobilePolicy;
            }

            // Varsayılan olarak WebSessionPolicy döndür
            var defaultPolicy = _policies.FirstOrDefault(p => p.ClientType.Equals("web", StringComparison.OrdinalIgnoreCase))
                             ?? _policies.FirstOrDefault();

            return defaultPolicy ?? new WebSessionPolicy();
        }
    }
}


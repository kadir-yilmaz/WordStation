namespace WordStation.BLL.Abstract
{
    public interface IClientSessionPolicyResolver
    {
        IClientSessionPolicy Resolve(string? clientType = null);
    }
}


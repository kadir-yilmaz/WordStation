using System.Linq.Expressions;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IRefreshTokenRepository
    {
        IQueryable<RefreshToken> GetByCondition(Expression<Func<RefreshToken, bool>> expression, bool trackChanges);
        void Create(RefreshToken entity);
        void Update(RefreshToken entity);
        void Save();
    }
}

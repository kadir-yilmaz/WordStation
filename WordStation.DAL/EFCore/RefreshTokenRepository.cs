using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<RefreshToken> GetByCondition(Expression<Func<RefreshToken, bool>> expression, bool trackChanges)
        {
            return trackChanges 
                ? _context.RefreshTokens.Where(expression) 
                : _context.RefreshTokens.Where(expression).AsNoTracking();
        }

        public void Create(RefreshToken entity) => _context.RefreshTokens.Add(entity);
        public void Update(RefreshToken entity) => _context.RefreshTokens.Update(entity);
        public void Save() => _context.SaveChanges();
    }
}

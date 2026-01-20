using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class SynonymGroupRepository : ISynonymGroupRepository
    {
        private readonly AppDbContext _context;

        public SynonymGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<SynonymGroup> GetAll(bool trackChanges) =>
            trackChanges 
                ? _context.SynonymGroups.Include(sg => sg.SynonymWords).ThenInclude(sw => sw.Word)
                : _context.SynonymGroups.Include(sg => sg.SynonymWords).ThenInclude(sw => sw.Word).AsNoTracking();

        public IQueryable<SynonymGroup> GetByCondition(Expression<Func<SynonymGroup, bool>> expression, bool trackChanges) =>
            trackChanges
                ? _context.SynonymGroups.Include(sg => sg.SynonymWords).ThenInclude(sw => sw.Word).Where(expression)
                : _context.SynonymGroups.Include(sg => sg.SynonymWords).ThenInclude(sw => sw.Word).Where(expression).AsNoTracking();

        public void Create(SynonymGroup entity) => _context.SynonymGroups.Add(entity);

        public void Update(SynonymGroup entity) => _context.SynonymGroups.Update(entity);

        public void Delete(SynonymGroup entity) => _context.SynonymGroups.Remove(entity);

        public void Save() => _context.SaveChanges();
    }
}

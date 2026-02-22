using System.Linq.Expressions;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface ISynonymGroupRepository
    {
        // Query
        IQueryable<SynonymGroup> GetAll(bool trackChanges);
        IQueryable<SynonymGroup> GetByCondition(Expression<Func<SynonymGroup, bool>> expression, bool trackChanges);
        
        // CRUD
        void Create(SynonymGroup entity);
        void Update(SynonymGroup entity);
        void Delete(SynonymGroup entity);
        
        void Save();
    }
}

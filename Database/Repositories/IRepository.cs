using System;using System.Collections.Generic;using System.Linq.Expressions;

namespace DataVisualizationApp.Database.Repositories
{
    public interface IRepository<TEntity>
    {
        TEntity? GetById(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        int Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(int id);
    }
}
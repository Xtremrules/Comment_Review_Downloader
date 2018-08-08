using Comment_Review_Downloader.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Interface
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null);

        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null);

        Task<T> GetOneAsync(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null);

        Task<T> GetFirstAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null);
        Task<bool> GetExistsAsync(Expression<Func<T, bool>> filter);
        void Create(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task SaveAsync();
    }
}

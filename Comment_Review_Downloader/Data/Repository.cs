using Comment_Review_Downloader.Data.Abstract;
using Comment_Review_Downloader.Data.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ILogger<Repository<T>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public Repository(IUnitOfWork unitOfWork, ILogger<Repository<T>> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public virtual void Create(T entity)
        {
            if (entity == null) throwError();
            _unitOfWork.Context.Set<T>().Add(entity);

        }

        protected virtual IQueryable<T> GetQueryable(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null, int? skip = null, int? take = null)
        {
            includeProperties = includeProperties ?? string.Empty;
            IQueryable<T> query = _unitOfWork.Context.Set<T>();

            if (filter != null) query = query.Where(filter);

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null) query = orderBy(query);

            if (skip.HasValue) query = query.Skip(skip.Value);

            if (take.HasValue) query = query.Take(take.Value);

            return query;
        }


        public virtual void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> GetExistsAsync(Expression<Func<T, bool>> filter = null)
        {
            return GetQueryable(filter).AnyAsync();
        }

        public virtual async Task<T> GetOneAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            return await GetQueryable(filter, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual async Task<T> GetFirstAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null)
        {
            return await GetQueryable(filter, orderBy, includeProperties).FirstOrDefaultAsync();
        }

        public virtual async Task SaveAsync()
        {
            await _unitOfWork.Commit();
        }

        public virtual void Update(T entity)
        {
            _unitOfWork.Context.Set<T>().Attach(entity);
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        void throwError()
        {
            throw new ArgumentNullException("entity", "can't pass a null object to this method");
        }
    }
}

using Comment_Review_Downloader.Data.Abstract;
using Comment_Review_Downloader.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data
{
    public class EntityRepo<T> : IEntityRepo<T> where T : BaseEntity
    {
        protected CommentsDbContext _context = new CommentsDbContext();
        //protected IContext _context;
        protected DbSet<T> _dbset;
        public EntityRepo(CommentsDbContext context)
        {
            _context = context;
            _dbset = _context.Set<T>();
        }

        public virtual async Task CreateAsync(T entity)
        {
            if (entity == null) throwError();
            _context.Entry<T>(entity).State = EntityState.Added;
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null) throwError();
            _dbset.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null) throwError();
            _context.Entry<T>(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbset.AsEnumerable<T>();
        }

        void throwError()
        {
            throw new ArgumentNullException("entity", "can't pass a null object to this method");
        }

        public virtual async Task<T> GetAsync(string url)
        {
            //throw new NotImplementedException("GetAsync(entity)");
            return await _dbset.FirstAsync();
        }
    }
}

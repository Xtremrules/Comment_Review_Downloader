using Comment_Review_Downloader.Data.Interface;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public CommentsDbContext Context { get; }

        public UnitOfWork(CommentsDbContext context)
        {
            Context = context;
        }

        public async Task Commit()
        {
            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}

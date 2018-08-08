using System;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Interface
{
    public interface IUnitOfWork: IDisposable
    {
        CommentsDbContext Context { get; }
        Task Commit();
    }
}

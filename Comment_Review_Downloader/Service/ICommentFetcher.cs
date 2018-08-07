using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service
{
    public interface ICommentFetcher
    {
        Task<string> FetchComments(ICommentsRequest request);
    }
}

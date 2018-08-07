using Comment_Review_Downloader.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Interface
{
    public interface IYouTubeCommentRepo: IEntityRepo<YouTube>
    {
    }
}

using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data
{
    public class YouTubeCommentRepo : EntityRepo<YouTube>, IYouTubeCommentRepo
    {
        public YouTubeCommentRepo(CommentsDbContext context) : base(context)
        {
        }

        public override Task UpdateAsync(YouTube entity)
        {
            entity.UpdatedDate = DateTime.Now;
            return base.UpdateAsync(entity);
        }

        public override async Task<YouTube> GetAsync(string url)
        {
            return await _context.YouTube.FirstOrDefaultAsync(x => x.YouTube_Id == url);
        }
    }
}

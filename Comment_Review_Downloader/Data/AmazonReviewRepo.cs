using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Data.Interface;

namespace Comment_Review_Downloader.Data
{
    public class AmazonReviewRepo : EntityRepo<Amazon>, IAmazonReviewRepo
    {
        public AmazonReviewRepo(CommentsDbContext context) : base(context)
        {
        }

        public override async Task<Amazon> GetAsync(string url)
        {
            return await _context.Amazon.FirstOrDefaultAsync(x => x.Url == url);
        }
    }
}

using Comment_Review_Downloader.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data
{
    public class CommentsDbContext : DbContext
    {
        public CommentsDbContext(DbContextOptions<CommentsDbContext> options) : base(options)
        {

        }

        public CommentsDbContext() { }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<CommentRequest> CommentRequests { get; set; }
    }
}

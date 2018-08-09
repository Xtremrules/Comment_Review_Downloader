using Comment_Review_Downloader.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Comment_Review_Downloader.Migrations
{
    [DbContext(typeof(CommentsDbContext))]
    partial class CommentsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("Comment_Review_Downloader.Data.Entity.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateAdded");

                    b.Property<bool>("Disabled");

                    b.Property<bool>("Fetched");

                    b.Property<string>("Location");

                    b.Property<int?>("NOC");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("UpdatedDate");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("Comment_Review_Downloader.Data.Entity.CommentRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CommentId");

                    b.Property<DateTime>("dateRequested");

                    b.Property<DateTime?>("dateSent");

                    b.Property<string>("emailAddress");

                    b.Property<bool>("emailed");

                    b.HasKey("Id");

                    b.HasIndex("CommentId");

                    b.ToTable("CommentRequests");
                });

            modelBuilder.Entity("Comment_Review_Downloader.Data.Entity.CommentRequest", b =>
                {
                    b.HasOne("Comment_Review_Downloader.Data.Entity.Comment", "Comment")
                        .WithMany("CommentRequests")
                        .HasForeignKey("CommentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

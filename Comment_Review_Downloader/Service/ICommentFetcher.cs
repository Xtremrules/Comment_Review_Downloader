﻿using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Models;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service
{
    public interface ICommentFetcher
    {
        Task<CommentDetails> FetchComments(Comment comment);
    }
}

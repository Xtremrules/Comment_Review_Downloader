using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Extensions;
using Comment_Review_Downloader.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service
{
    public abstract class CommentFetcher: ICommentFetcher
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger<CommentFetcher> _logger;
        protected readonly IConfiguration _config;
        protected readonly string _path;

        public CommentFetcher(ILogger<CommentFetcher> logger,
            IConfiguration config)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _config = config;
            _path = AppConstants.FileDirectory;
        }

        public virtual async Task<CommentDetails> FetchComments(Comment comment)
        {
            var response = await _httpClient.GetAsync(comment.Url);
            var fileName = Guid.NewGuid().ToString() + ".csv";
            var fullFilePath = Path.Combine(_path, fileName);
            await response.Content.ReadAsFileAsync(fullFilePath, true);
            _logger.LogInformation($"fetching successfull for {comment.Url}, {response.StatusCode}");
            //return fullFilePath;
            return new CommentDetails { Filename = fullFilePath };
        }
    }
}

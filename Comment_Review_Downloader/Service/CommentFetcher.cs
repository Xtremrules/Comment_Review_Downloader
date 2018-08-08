using Comment_Review_Downloader.Extensions;
using Comment_Review_Downloader.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service
{
    public class CommentFetcher: ICommentFetcher
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

        public virtual async Task<string> FetchComments(ICommentsRequest request)
        {
            var response = await _httpClient.GetAsync(request.RequestUrl);
            var fileName = Guid.NewGuid().ToString() + "csv";
            var fullFilePath = Path.Combine(_path, fileName);
            await response.Content.ReadAsFileAsync(fullFilePath, true);
            _logger.LogInformation($"fetching successfull for {request.RequestUrl}, {response.StatusCode}");
            return fullFilePath;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Extensions;
using Comment_Review_Downloader.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Comment_Review_Downloader.Service.HostedServices
{
    public class YoutubeCommentsFetcher : CommentFetcher
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public YoutubeCommentsFetcher(ILogger<CommentFetcher> logger, IConfiguration config) : base(logger, config)
        {
            _apiKey = _config["Youtube:ApiKey"];
            _endpoint = $"https://www.googleapis.com/youtube/v3/commentThreads?key={_apiKey}&part=snippet&maxResults=100";
        }

        //public override async Task<string> FetchComments(Data.Entity.Comment comment)
        public override async Task<CommentDetails> FetchComments(Data.Entity.Comment comment)
        {
            #region initialize
            var videoId = GetVideoId(comment.Url);
            var query = BuildQuery(videoId);
            var response = await _httpClient.GetAsync(query);
            #endregion

            #region Failed Fetch
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return default(CommentDetails);
            }
            #endregion

            #region needed params
            var fileName = Guid.NewGuid().ToString().Replace("-","").ToUpper() + ".csv";
            var currentResponse = new YouTubeCommentSet();
            bool hasMore = false;
            string nextPageToken = "";
            var refinedComments = new List<RefinedComment>();
            #endregion

            do
            {
                if (hasMore)
                {
                    var newQuery = AddNextPageToken(query, nextPageToken);
                    response = await _httpClient.GetAsync(newQuery);
                }
                currentResponse = await response.Content.ReadAsAsync<YouTubeCommentSet>();
                var snippets = currentResponse.Items.Select(e => GetRefinedComment(e, videoId));
                refinedComments.AddRange(snippets);
                nextPageToken = currentResponse.NextPageToken;
                hasMore = !string.IsNullOrEmpty(nextPageToken);
            } while (hasMore);

            var fileSaved = SaveToFile(fileName, refinedComments);
            _logger.LogInformation($"fetching successfull for {comment.Url}, {response.StatusCode}");
            var details = new CommentDetails
            {
                Filename = fileSaved ? fileName : "",
                Name = await VideoName(videoId),
                NOC = refinedComments.Count
            };
            return details;
            //return fileSaved ? fileName : "";
        }

        private RefinedComment GetRefinedComment(Comment comment, string videoId)
        {
            return new RefinedComment
            {
                Username = comment.Snippet.TopLevelComment.Snippet.AuthorDisplayName,
                Date = comment.Snippet.TopLevelComment.Snippet.PublishedAt.ToLongDateString(),
                Rating = comment.Snippet.TopLevelComment.Snippet.ViewerRating,
                Comment = comment.Snippet.TopLevelComment.Snippet.TextOriginal,
                Link = GetDirectCommentLink(videoId, comment.Snippet.TopLevelComment.Id)
            };
        }

        public bool SaveToFile(string fileName, List<RefinedComment> comments)
        {
            var writer = new CsvWriter();
            var fullFilePath = Path.Combine(_path, fileName);

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            return writer.Write(comments, fullFilePath, true);
        }

        public string GetVideoId(string url)
        {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(url)?.Query);
            var videoId = query.GetValueOrDefault("v");
            return videoId;
        }

        public string GetDirectCommentLink(string videoId, string commentId)
        {
            return $"https://www.youtube.com/watch?v={videoId}&lc={commentId}";
        }

        public string BuildQuery(string videoId)
        {
            return $"{_endpoint}&videoId={videoId}";
        }

        public string AddNextPageToken(string url, string nextPageToken)
        {
            return $"{url}&pageToken={nextPageToken}";
        }

        public async Task<string> VideoName(string videoId)
        {
            var query = $"https://www.googleapis.com/youtube/v3/videos?part=snippet&id={videoId}&key={_apiKey}";
            var response = await _httpClient.GetAsync(query);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(responseString);
            var items = (JArray)data["items"];
            var first = (JObject)items[0]["snippet"];
            var name = first["title"].ToString();
            return name;
            //return this.http.get(this.url + 'videos?part=statistics&id=' + videoid + '&key=' + this.key);
        }

        private class YouTubeCommentSet
        {
            public string NextPageToken { get; set; }
            public PageInfo PageInfo { get; set; }
            public IList<Comment> Items { get; set; }
        }

        private class PageInfo
        {
            public int TotalResults { get; set; }
        }

        private class Comment
        {
            public string Id { get; set; }
            public ParentSnippet Snippet { get; set; }

        }

        private class ParentSnippet
        {
            public string VideoId { get; set; }
            public TopLevelComment TopLevelComment { get; set; }
        }

        private class TopLevelComment
        {
            public string Id { get; set; }
            public ChildSnippet Snippet { get; set; }
        }

        private class ChildSnippet
        {
            public string AuthorDisplayName { get; set; }
            public DateTime PublishedAt { get; set; }
            public string ViewerRating { get; set; }
            public string TextOriginal { get; set; }
        }
    }
}

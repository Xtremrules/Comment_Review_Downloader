using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Data.Interface;
using Comment_Review_Downloader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Data.Entity.Comment> _commentRepo;
        private readonly IRepository<CommentRequest> _commentRequestRepo;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public HomeController(IRepository<Data.Entity.Comment> comment, IRepository<CommentRequest> commentRequest,
            ILogger<HomeController> logger, IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _commentRepo = comment;
            _commentRequestRepo = commentRequest;
            _logger = logger;
            _apiKey = config["Youtube:ApiKey"];
            _endpoint = "https://www.googleapis.com/youtube/v3/commentThreads?key=" + _apiKey + "&part=snippet&maxResults=20&videoId={0}";
        }

        public IActionResult Index()
        {
            return View(new RequestViewModel());
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Index(RequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.RequestUrl.Contains("amazon"))
            {
                var regEx = "(?:dp|product|o|gp|-)\\/(B[0-9]{2}[0-9A-Z]{7}|[0-9]{9}(?:X|[0-9]))";
                var Match = Regex.Match(model.RequestUrl, regEx, RegexOptions.IgnoreCase);
                if (Match.Success)
                {
                    var d = Match.Value;
                    model.RequestUrl = "https://www.amazon.com/product-reviews/" + Match.Value.Substring(Match.Value.Length - 10);
                    try
                    {
                        await AddRequesAsync(model);
                        return View("amazon-download", model);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    AddError("Not a valid Amazon Product page");
                    return View(model);
                }
            }
            else if (model.RequestUrl.Contains("youtube"))
            {
                var regEx = "^(http(s)??\\:\\/\\/)?(www\\.|m\\.)?((youtube\\.com\\/watch\\?v=)|(youtu.be\\/))([a-zA-Z0-9\\-_]{11})$";
                var isMatch = Regex.IsMatch(model.RequestUrl, regEx, RegexOptions.IgnoreCase);
                if (isMatch)
                {
                    try
                    {
                        await AddRequesAsync(model);
                        model.RequestUrl = model.RequestUrl.Substring(model.RequestUrl.Length - 11);
                        await GetYouTubeCommentAsync(model.RequestUrl);
                        return View("youtube-download", model);
                    }
                    catch (Exception ex)
                    {
                        AddError(ex.Message);
                        return View(model);
                    }
                }
                else
                {
                    AddError("Not a Valid YouTube Address");
                    return View(model);
                }

            }

            AddError("Not an Amazon or YouTube address");
            return View(model);
        }

        async Task AddRequesAsync(RequestViewModel model)
        {
            var comment = await _commentRepo.GetOneAsync(x => x.Url == model.RequestUrl);
            if (comment != null)
            {
                if(comment.Fetched)
                    if(comment.UpdatedDate?.AddHours(2).Date <= DateTime.Now.Date)
                    {
                        comment.Fetched = false;
                        _commentRepo.Update(comment);
                    }
                _commentRequestRepo.Create(new CommentRequest
                {
                    dateRequested = DateTime.Now,
                    emailAddress = model.Email,
                    emailed = false,
                    CommentId = comment.Id,
                });
                await _commentRequestRepo.SaveAsync();
            }
            else
            {
                comment = new Data.Entity.Comment
                {
                    Url = model.RequestUrl,
                    DateAdded = DateTime.Now,
                    Fetched = false,
                    Disabled = false
                };

                comment.CommentRequests = new List<CommentRequest>
                {
                    new CommentRequest
                    {
                        dateRequested = DateTime.Now,
                        emailAddress = model.Email,
                        emailed = false,
                    }
                };
                _commentRepo.Create(comment);
                await _commentRepo.SaveAsync();
            }
        }

        private void AddError(string message)
        {
            TempData["error"] = message;
        }

        private async Task GetYouTubeCommentAsync(string videoId)
        {
            var query = string.Format(_endpoint, videoId);
            var response = await _httpClient.GetAsync(query);

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return;

            var currentResponse = new YouTubeCommentSet();
            var refinedComments = new List<RefinedComment>();

            currentResponse = await response.Content.ReadAsAsync<YouTubeCommentSet>();
            var snippets = currentResponse.Items.Select(e => GetRefinedComment(e, videoId));
            refinedComments.AddRange(snippets);

            ViewBag.refinedComments = refinedComments;
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

        public string GetDirectCommentLink(string videoId, string commentId)
        {
            return $"https://www.youtube.com/watch?v={videoId}&lc={commentId}";
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Dublication

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

        #endregion
    }
}

using Comment_Review_Downloader.Data.Entity;
using Comment_Review_Downloader.Extensions;
using Comment_Review_Downloader.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service.HostedServices
{
    public class AmazonReviewFetcher : CommentFetcher
    {
        public AmazonReviewFetcher(ILogger<CommentFetcher> logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
        {
        }

        public override async Task<CommentDetails> FetchComments(Comment comment)
        {
            var refinedComments = new List<RefinedComment>();
            var fileName = Guid.NewGuid().ToString().Replace("-", "").ToUpper() + ".csv";
            var title = "";
            var webScrap = new HtmlWeb();
            var hasMore = false;
            var count = 1;

            do
            {
                #region Initialize
                if (hasMore)
                    comment.Url = comment.Url + "/?pageNumber=" + count.ToString();

                var htmlScrap = await webScrap.LoadFromWebAsync(comment.Url);

                var author = htmlScrap.DocumentNode.CssSelect(".author").ToList();
                var date = htmlScrap.DocumentNode.CssSelect(".review-date").ToList();
                var rate = htmlScrap.DocumentNode.CssSelect("i.review-rating > span").ToList();
                var text = htmlScrap.DocumentNode.CssSelect(".review-text").ToList();

                var last = htmlScrap.DocumentNode.CssSelect(".a-disabled.a-last").FirstOrDefault();

                if (!hasMore)
                    title = htmlScrap.DocumentNode.CssSelect("title").FirstOrDefault().InnerHtml;
                #endregion

                #region Nomalize

                var authorNo = author.Count;
                var dateNo = date.Count;
                var rateNo = rate.Count;
                var textNo = text.Count;

                if (authorNo > textNo)
                    author = author.Skip(authorNo - textNo).ToList();

                if (dateNo > textNo)
                    date = date.Skip(dateNo - textNo).ToList();

                if (rateNo > textNo)
                    rate = rate.Skip(rateNo - textNo).ToList();
                #endregion

                #region Add To RefinedComment
                for (int i = 0; i < textNo; i++)
                {
                    refinedComments.Add(new RefinedComment
                    {
                        Comment = text[i].InnerHtml,
                        Date = date[i].InnerHtml.Replace("on ", ""),
                        Link = "",
                        Rating = rate[i].InnerHtml,
                        Username = author[i].InnerHtml,
                    });
                }
                #endregion

                count++;
                hasMore = last == null ? false : true;
            } while (hasMore);

            var fileSaved = SaveToFile(fileName, refinedComments);
            _logger.LogInformation($"fetching successfull for {comment.Url}, {webScrap.StatusCode}");

            var details = new CommentDetails
            {
                Filename = fileSaved ? fileName : "",
                Name = title,
                NOC = refinedComments.Count
            };
            return details;
        }

        public bool SaveToFile(string fileName, List<RefinedComment> comments)
        {
            var writer = new CsvWriter();
            var fullFilePath = Path.Combine(_path, fileName);

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            return writer.Write(comments, fullFilePath, true);
        }
    }
}

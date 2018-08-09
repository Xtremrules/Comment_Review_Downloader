﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Comment_Review_Downloader.Models;
using Comment_Review_Downloader.Data.Interface;
using Microsoft.Extensions.Logging;
using Comment_Review_Downloader.Data.Entity;

namespace Comment_Review_Downloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IRepository<CommentRequest> _commentRequestRepo;

        public HomeController(IRepository<Comment> comment, IRepository<CommentRequest> commentRequest, ILogger<HomeController> logger)
        {
            _commentRepo = comment;
            _commentRequestRepo = commentRequest;
            _logger = logger;
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
                    addError("Not a valid Amazon Product page");
                    return View(model);
                }
            }
            else if(model.RequestUrl.Contains("youtube"))
            {
                var regEx = "^(http(s)??\\:\\/\\/)?(www\\.|m\\.)?((youtube\\.com\\/watch\\?v=)|(youtu.be\\/))([a-zA-Z0-9\\-_]{11})$";
                var isMatch = Regex.IsMatch(model.RequestUrl, regEx, RegexOptions.IgnoreCase);
                if (isMatch)
                {
                    try
                    {
                        await AddRequesAsync(model);
                        return View("youtube-download", model);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    addError("Not a Valid YouTube Address");
                    return View(model);
                }

            }

            addError("Not an Amazon or YouTube address");
            return View(model);
        }

        async Task AddRequesAsync(RequestViewModel model)
        {
            var comment = await _commentRepo.GetOneAsync(x => x.Url == model.RequestUrl);
            if (comment != null)
            {
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
                comment = new Comment
                {
                    Url = model.RequestUrl,
                    DateAdded = DateTime.Now,
                    Fetched = false,
                    Disabled = false
                };

                comment.CommentRequests = new List<CommentRequest>();
                comment.CommentRequests.Add(new CommentRequest
                {
                    dateRequested = DateTime.Now,
                    emailAddress = model.Email,
                    emailed = false,
                });
                _commentRepo.Create(comment);
                await _commentRepo.SaveAsync();
            }
        }

        private void addError(string message)
        {
            TempData["error"] = message;
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
    }
}

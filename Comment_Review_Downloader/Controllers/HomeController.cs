using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Comment_Review_Downloader.Models;

namespace Comment_Review_Downloader.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new RequestViewModel());
        }

        [ValidateAntiForgeryToken, HttpPost]
        public IActionResult Index(RequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.RequestUrl.Contains("amazon"))
            {
                return View("amazon-download", model.RequestUrl);
            }
            else
            {
                var match = "^(http(s)??\\:\\/\\/)?(www\\.|m\\.)?((youtube\\.com\\/watch\\?v=)|(youtu.be\\/))([a-zA-Z0-9\\-_]{11})$";
                var isMatch = Regex.IsMatch(model.RequestUrl, match, RegexOptions.IgnoreCase);
                if (isMatch)
                {
                    var youtubeId = model.RequestUrl.Substring(model.RequestUrl.Length - 11);
                    return View("youtube-download", model.RequestUrl);
                }

                return View(model);
            }
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

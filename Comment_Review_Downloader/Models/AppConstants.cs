using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Models
{
    public class AppConstants
    {
        public const string Youtube = "youtube";
        public const string Amazon = "amazon";
        public const string YoutubeHost = "www.youtube.com";
        public const string AmazonHost = "www.amazon.com";
        public static string FileDirectory { get => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CommentFolder"); }
    }
}

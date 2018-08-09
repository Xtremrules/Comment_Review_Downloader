using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Models
{
    public class RefinedComment
    {
        public string Username { get; set; }
        public string Date { get; set; }
        public string Rating { get; set; }
        public string Comment { get; set; }
        public string Link { get; set; }
    }
}

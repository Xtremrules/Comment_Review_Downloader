using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Entity
{
    public class Amazon
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Filename { get; set; }
        public DateTime Date { get; set; }
    }
}

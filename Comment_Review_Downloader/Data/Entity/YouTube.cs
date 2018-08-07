using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Entity
{
    public class YouTube
    {
        public int Id { get; set; }
        public string YouTube_Id { get; set; }
        public int NOC { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime DateAdded { get; set; }
    }
}

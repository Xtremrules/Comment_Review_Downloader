using Comment_Review_Downloader.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Entity
{
    public class Amazon: BaseEntity
    {
        public int Id { get; set; }
        public string Url { get; set; }
        /// <summary>
        /// No of Reviews
        /// </summary>
        public int NOC { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }
}

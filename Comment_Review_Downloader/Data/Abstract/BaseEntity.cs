using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Abstract
{
    public class BaseEntity
    {
        public DateTime? UpdatedDate { get; set; }
        public DateTime DateAdded { get; set; }
    }
}

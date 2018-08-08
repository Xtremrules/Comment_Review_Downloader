using Comment_Review_Downloader.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Data.Entity
{
    public class CommentRequest: BaseEntity
    {
        public int Id { get; set; }
        public string emailAddress { get; set; }
        public bool emailed { get; set; }
        public DateTime? dateSent { get; set; }
        public DateTime dateRequested { get; set; }

        public int CommentId { get; set; }
        public virtual Comment Comment { get; set; }
    }
}

using Comment_Review_Downloader.Data.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Comment_Review_Downloader.Data.Entity
{
    public class Comment : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Url { get; set; }
        /// <summary>
        /// No of Comments
        /// </summary>
        public int? NOC { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public bool Fetched { get; set; }
        public bool Disabled { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual ICollection<CommentRequest> CommentRequests { get; set; }

        public override string ToString()
        {
            return $"Id:{Id}, Url:{Url}, Fetched:{Fetched.ToString()}, Name:{Name}, Location: {Location}, Disabled: {Disabled.ToString()}";
        }
    }
}

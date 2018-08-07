using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Models
{
    public class RequestViewModel
    {
        [Required]
        public string RequestUrl { get; set; }
        [Required]
        public string Email { get; set; }
    }
}

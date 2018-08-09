using System.ComponentModel.DataAnnotations;

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

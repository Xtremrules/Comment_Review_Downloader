using Comment_Review_Downloader.Data.Abstract;

namespace Comment_Review_Downloader.Data.Entity
{
    public class YouTube : BaseEntity
    {
        public int Id { get; set; }
        public string YouTube_Id { get; set; }
        /// <summary>
        /// No of Comments
        /// </summary>
        public int NOC { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }
}

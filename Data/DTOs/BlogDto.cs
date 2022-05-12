namespace RazorBlog.Data.DTOs
{
    public record BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string AuthorName { get; set; }
        public uint ViewCount { get; set; }
        public string Introduction { get; set; }
        public System.DateTime Date { get; set; }
        public string CoverImageUri { get; set; }
    }
}
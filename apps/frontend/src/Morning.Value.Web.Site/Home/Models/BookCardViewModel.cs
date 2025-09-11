namespace Morning.Value.Web.Site.Home.Models
{
    public class BookCardViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Available { get; set; }
    }
}

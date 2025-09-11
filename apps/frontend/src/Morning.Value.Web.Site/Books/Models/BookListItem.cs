namespace Morning.Value.Web.Site.Books.Models
{
    public class BookListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Genre { get; set; } = "";
        public int AvailableCopies { get; set; }
    }
}

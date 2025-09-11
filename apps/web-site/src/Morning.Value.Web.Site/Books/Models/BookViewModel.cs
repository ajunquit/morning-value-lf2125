namespace Morning.Value.Web.Site.Books.Models
{
    public class BookViewModel
    {
        public Guid Id { get; set; }
        public string  Title { get; set; }
        public string Author { get; set; }
        public string Gender { get; set; }
        public int Stock { get; set; }
    }
}

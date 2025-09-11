namespace Morning.Value.Web.Site.Home.Models
{
    public class BookCardViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Available { get; set; } // copias disponibles (>=0)
    }
}

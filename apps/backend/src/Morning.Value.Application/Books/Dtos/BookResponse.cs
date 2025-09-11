namespace Morning.Value.Application.Books.Dtos
{
    public class BookResponse
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int AvailableCopies { get; set; }
    }
}

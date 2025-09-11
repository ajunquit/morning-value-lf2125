using Morning.Value.Web.Site.Common.Models;

namespace Morning.Value.Web.Site.Books.Models
{
    public class BookManagementViewModel
    {
        // Form del modal "Nuevo"
        public BookCreateViewModel Create { get; set; } = new();

        // Grilla de consulta
        public PagedResult<BookListItem> Grid { get; set; } = new();

        // Filtro actual (texto libre)
        public string? Query { get; set; }
    }
}

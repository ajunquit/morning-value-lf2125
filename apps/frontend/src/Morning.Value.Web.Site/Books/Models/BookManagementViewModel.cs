using Morning.Value.Application.Books.Dtos;
using Morning.Value.Application.Common.Dtos;

namespace Morning.Value.Web.Site.Books.Models
{
    public class BookManagementViewModel
    {
        // Form del modal "Nuevo"
        public BookCreateViewModel Create { get; set; } = new();

        // Grilla de consulta
        public PagedResult<BookResponse> Grid { get; set; } = new();

        // Filtro actual (texto libre)
        public string? Query { get; set; }
    }
}

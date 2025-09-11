using Morning.Value.Application.Books.Dtos;
using Morning.Value.Application.Common.Dtos;

namespace Morning.Value.Web.Site.Books.Models
{
    public class BookManagementViewModel
    {
        public BookCreateViewModel Create { get; set; } = new();

        public PagedResult<BookResponse> Grid { get; set; } = new();

        public string? Query { get; set; }
    }
}

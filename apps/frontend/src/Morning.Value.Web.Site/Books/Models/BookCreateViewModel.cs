using System.ComponentModel.DataAnnotations;

namespace Morning.Value.Web.Site.Books.Models
{
    public class BookCreateViewModel
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Author { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string Genre { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Debe ser >= 0")]
        public int AvailableCopies { get; set; }
    }
}

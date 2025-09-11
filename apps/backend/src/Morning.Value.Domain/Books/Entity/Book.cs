using Morning.Value.Domain.Common.Entities;
using Morning.Value.Domain.Exceptions;

namespace Morning.Value.Domain.Books.Entity
{
    public class Book: AuditableEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;
        public int AvailableCopies { get; private set; }

        private Book() { } 

        public Book(string title, string author, string genre, int availableCopies)
        {
            SetTitle(title);
            SetAuthor(author);
            SetGenre(genre);
            SetAvailableCopies(availableCopies);
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("El título es obligatorio.");
            Title = title.Trim();
        }

        public void SetAuthor(string author)
        {
            if (string.IsNullOrWhiteSpace(author)) throw new DomainException("El autor es obligatorio.");
            Author = author.Trim();
        }

        public void SetGenre(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre)) throw new DomainException("El género es obligatorio.");
            Genre = genre.Trim();
        }

        public void SetAvailableCopies(int count)
        {
            if (count < 0) throw new DomainException("Las copias disponibles no pueden ser negativas.");
            AvailableCopies = count;
        }

        public bool HasAvailability(int quantity = 1) => AvailableCopies >= quantity;

        public void ReserveOne()
        {
            if (!HasAvailability()) throw new DomainException("No hay copias disponibles para préstamo.");
            AvailableCopies -= 1;
        }

        public void ReturnOne()
        {
            AvailableCopies += 1;
        }
    }
}

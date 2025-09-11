using FluentAssertions;
using Morning.Value.Domain.Exceptions;

namespace Morning.Value.Domain.Test
{
    public class BookTests
    {
        [Fact]
        public void Ctor_Valido_InicializaPropiedades()
        {
            // arrange + act
            var book = new Books.Entity.Book("Clean Architecture", "Robert C. Martin", "Tech", 2);

            // assert
            book.Title.Should().Be("Clean Architecture");
            book.Author.Should().Be("Robert C. Martin");
            book.Genre.Should().Be("Tech");
            book.AvailableCopies.Should().Be(2);
        }

        [Theory]
        [InlineData("", "Autor", "Genero", 1)]
        [InlineData("Titulo", "", "Genero", 1)]
        [InlineData("Titulo", "Autor", "", 1)]
        public void Ctor_Invalido_LanzaDomainException(string title, string author, string genre, int copies)
        {
            Action act = () => new Books.Entity.Book(title, author, genre, copies);
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ReserveOne_DisminuyeDisponibilidad_Y_FallaSinStock()
        {
            var book = new Books.Entity.Book("DDD", "Evans", "Tech", 2);

            book.ReserveOne();
            book.AvailableCopies.Should().Be(1);

            book.ReserveOne();
            book.AvailableCopies.Should().Be(0);

            Action act = () => book.ReserveOne();
            act.Should().Throw<DomainException>()
               .WithMessage("*No hay copias disponibles*");
        }

        [Fact]
        public void ReturnOne_IncrementaDisponibilidad()
        {
            var book = new Books.Entity.Book("DDD", "Evans", "Tech", 0);

            book.ReturnOne();
            book.AvailableCopies.Should().Be(1);
        }
    }
}

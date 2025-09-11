using FluentAssertions;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Domain.Users.Enums;

namespace Morning.Value.Domain.Test
{
    public class UserTests
    {
        [Fact]
        public void Ctor_Valido_InicializaPropiedades_Y_RespetaDefaults()
        {
            // arrange
            var name = "  Jane Doe  ";
            var email = "  jane@acme.com ";
            var hash = "  HASHED-123  ";

            // act
            var user = new User(name, email, hash);

            // assert
            user.Name.Should().Be("Jane Doe");              // trimmed
            user.Email.Should().Be("jane@acme.com");        // trimmed
            user.PasswordHash.Should().Be("HASHED-123");    // trimmed
            user.Role.Should().Be(RoleType.Reader);         // default
        }

        [Fact]
        public void Ctor_ConRol_Admin_AsignaRol()
        {
            var user = new User("John", "john@acme.com", "HASH", RoleType.Admin);
            user.Role.Should().Be(RoleType.Admin);
        }

        [Theory]
        [InlineData(null, "mail@acme.com", "HASH")]         // nombre inválido
        [InlineData("   ", "mail@acme.com", "HASH")]
        [InlineData("John", null, "HASH")]                  // email inválido
        [InlineData("John", "   ", "HASH")]
        [InlineData("John", "mail@acme.com", null)]         // hash inválido
        [InlineData("John", "mail@acme.com", "   ")]
        public void Ctor_Invalido_LanzaDomainException(string name, string email, string hash)
        {
            Action act = () => new User(name, email, hash);
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void SetName_Valido_Trimea_Y_Asigna()
        {
            var u = new User("A", "a@a.com", "H");
            u.SetName("  Nuevo Nombre  ");
            u.Name.Should().Be("Nuevo Nombre");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_Invalido_LanzaDomainException(string value)
        {
            var u = new User("A", "a@a.com", "H");
            Action act = () => u.SetName(value);
            act.Should().Throw<DomainException>()
               .WithMessage("*obligatorio*");
        }

        [Fact]
        public void SetEmail_Valido_Trimea_Y_Asigna()
        {
            var u = new User("A", "a@a.com", "H");
            u.SetEmail("  user@domain.com  ");
            u.Email.Should().Be("user@domain.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetEmail_Invalido_LanzaDomainException(string value)
        {
            var u = new User("A", "a@a.com", "H");
            Action act = () => u.SetEmail(value);
            act.Should().Throw<DomainException>()
               .WithMessage("*obligatorio*");
        }

        [Fact]
        public void SetPasswordHash_Valido_Asigna()
        {
            var u = new User("A", "a@a.com", "H");
            u.SetPasswordHash("  NEW-HASH  ");
            u.PasswordHash.Should().Be("NEW-HASH");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetPasswordHash_Invalido_LanzaDomainException(string value)
        {
            var u = new User("A", "a@a.com", "H");
            Action act = () => u.SetPasswordHash(value);
            act.Should().Throw<DomainException>()
               .WithMessage("*inválido*");
        }

        [Fact]
        public void SetRole_CambiaElRol()
        {
            var u = new User("A", "a@a.com", "H");
            u.Role.Should().Be(RoleType.Reader);

            u.SetRole(RoleType.Admin);
            u.Role.Should().Be(RoleType.Admin);
        }
    }
}

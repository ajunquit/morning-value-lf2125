using FluentAssertions;
using Moq;
using Morning.Value.Application.Common.Services;
using Morning.Value.Application.Users.Dtos;
using Morning.Value.Application.Users.Services;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Users.Entity;
using Morning.Value.Domain.Users.Enums;
using Morning.Value.Domain.Users.Interfaces;

namespace Morning.Value.Application.Test.Auth
{
    public class AuthAppService_Tests
    {
        private readonly Mock<IUnitOfWorkAsync> _uow = new();
        private readonly Mock<IUserRepositoryAsync> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();

        private readonly AuthAppService _sut;

        public AuthAppService_Tests()
        {
            _uow.SetupGet(x => x.UserRepository).Returns(_userRepo.Object);
            _sut = new AuthAppService(_uow.Object, _hasher.Object);
        }

        // ========== SignUpAsync ==========

        [Fact]
        public async Task SignUp_WhenEmailIsUnique_HashesAndSaves_ReturnsSuccess()
        {
            // arrange
            var name = "Jane";
            var email = "jane@acme.com";
            var password = "P@ssw0rd!";
            var role = RoleType.Reader;

            _userRepo.Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            _hasher.Setup(h => h.Hash(password)).Returns("HASHED");

            _userRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
                     .ReturnsAsync(true);

            _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // act
            RegisterResult result = await _sut.SignUpAsync(name, email, password, role);

            // assert
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();

            _userRepo.Verify(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _hasher.Verify(h => h.Hash(password), Times.Once);
            _userRepo.Verify(r => r.InsertAsync(It.Is<User>(u =>
                u.Name == name &&
                u.Email == email &&
                u.PasswordHash == "HASHED" &&
                u.Role == role
            )), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SignUp_WhenEmailAlreadyExists_ReturnsError_AndDoesNotInsert()
        {
            var email = "dup@acme.com";

            _userRepo.Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            var result = await _sut.SignUpAsync("John", email, "secret", RoleType.Reader);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("ya está registrado");

            _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
            _userRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ========== SignInAsync ==========

        [Fact]
        public async Task SignIn_WhenUserExists_AndPasswordMatches_ReturnsSuccessWithUserInfo()
        {
            var email = "user@acme.com";
            var user = new User("User", email, "HASH", RoleType.Admin);
            // (No necesitamos setear Id; la prueba valida que se propaga tal cual)

            _userRepo.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            _hasher.Setup(h => h.Verify("HASH", "secret")).Returns(true);

            AuthResult result = await _sut.SignInAsync(email, "secret");

            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Name.Should().Be("User");
            result.Email.Should().Be(email);
            result.Role.Should().Be(RoleType.Admin);

            _userRepo.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
            _hasher.Verify(h => h.Verify("HASH", "secret"), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SignIn_WhenUserDoesNotExist_ReturnsError_AndDoesNotVerifyPassword()
        {
            var email = "nouser@acme.com";

            _userRepo.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User?)null);

            var result = await _sut.SignInAsync(email, "secret");

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("inválidos");

            _hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SignIn_WhenPasswordIsInvalid_ReturnsError()
        {
            var email = "user@acme.com";
            var user = new User("User", email, "HASH");

            _userRepo.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);

            _hasher.Setup(h => h.Verify("HASH", "badpass")).Returns(false);

            var result = await _sut.SignInAsync(email, "badpass");

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("inválidos");

            _hasher.Verify(h => h.Verify("HASH", "badpass"), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

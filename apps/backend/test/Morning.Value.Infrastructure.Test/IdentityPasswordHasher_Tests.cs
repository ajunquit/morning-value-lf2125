using FluentAssertions;
using Morning.Value.Infrastructure.Security;

namespace Morning.Value.Infrastructure.Test
{
    public class IdentityPasswordHasher_Tests
    {
        [Fact]
        public void Hash_Returns_NonEmpty_And_Not_Plain()
        {
            // arrange
            var hasher = new IdentityPasswordHasher();
            var pwd = "SúperSecreta#2025";

            // act
            var hash = hasher.Hash(pwd);

            // assert
            hash.Should().NotBeNullOrWhiteSpace();
            hash.Should().NotBe(pwd);                         // no debe quedar en texto plano
            hash.Length.Should().BeGreaterThan(20);           // heurística: hash “largo”
        }

        [Fact]
        public void Verify_ReturnsTrue_For_CorrectPassword()
        {
            var hasher = new IdentityPasswordHasher();
            var pwd = "P@ssw0rd!";
            var hash = hasher.Hash(pwd);

            var ok = hasher.Verify(hash, pwd);

            ok.Should().BeTrue();
        }

        [Fact]
        public void Verify_ReturnsFalse_For_WrongPassword()
        {
            var hasher = new IdentityPasswordHasher();
            var pwd = "Correcta123";
            var hash = hasher.Hash(pwd);

            var ok = hasher.Verify(hash, "incorrecta");

            ok.Should().BeFalse();
        }

        [Fact]
        public void Hash_IsSalted_TwoHashes_ForSamePassword_AreDifferent()
        {
            var hasher = new IdentityPasswordHasher();
            var pwd = "MismaClave#";

            var h1 = hasher.Hash(pwd);
            var h2 = hasher.Hash(pwd);

            // Con Identity, el salt es aleatorio => hashes diferentes
            h1.Should().NotBe(h2);
            hasher.Verify(h1, pwd).Should().BeTrue();
            hasher.Verify(h2, pwd).Should().BeTrue();
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Morning.Value.Application.Common.Services;

namespace Morning.Value.Infrastructure.Security
{
    public class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password) =>
            _hasher.HashPassword(null!, password);

        public bool Verify(string hashedPassword, string providedPassword)
        {
            var r = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
            return r == PasswordVerificationResult.Success ||
                   r == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}

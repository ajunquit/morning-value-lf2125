using Morning.Value.Application.Common.Services;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Auth.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        public string? UserId => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? Email => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        public string? UserName => _http.HttpContext?.User?.Identity?.Name;
    }
}

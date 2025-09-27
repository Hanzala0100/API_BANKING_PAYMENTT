using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly BankDbContext _context;

    public UserContext(IHttpContextAccessor httpContextAccessor, BankDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public User GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
            throw new Exception("No HttpContext available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new Exception("User not logged in");

        var userId = long.Parse(userIdClaim);
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            throw new Exception("User not found");
        var clientIdClaim = httpContext.User.FindFirst("ClientId")?.Value;
        if (!string.IsNullOrEmpty(clientIdClaim) && long.TryParse(clientIdClaim, out var clientId))
        {
            user.ClientId = clientId;
        }

        var bankIdClaim = httpContext.User.FindFirst("BankId")?.Value;
        if (!string.IsNullOrEmpty(bankIdClaim) && long.TryParse(bankIdClaim, out var bankId))
        {
            user.BankId = bankId;
        }

        return user;
    }
}


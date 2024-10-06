using System;
using System.IdentityModel.Tokens.Jwt;
using Library.Domain.Sessions;
using Library.Domain.Users.Entities;
using Library.Domain.Users.Repositories;

namespace Library.Presentation.Authentication;

public class SessionsManager(
    IUsersRepository usersRepository,
    IHttpContextAccessor httpContextAccessor
) : ISessionsManager
{
    public int? GetCurrentUserId()
    {
        var ctx = httpContextAccessor.HttpContext;

        if (ctx is null) return null;
        if (!ctx.Request.Headers.TryGetValue("Authorization", out var headerAuth)) return null;

        var jwtTokenAsString = headerAuth.First()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtTokenAsString);

        return int.TryParse(jwtToken.Claims.SingleOrDefault(c => c.Type == "UserId")?.Value, out var userId) ? userId : null;
    }

    public async Task<User> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return null;
        return await usersRepository.Get(userId.Value);
    }
}

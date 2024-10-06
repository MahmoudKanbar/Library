using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library.Domain.Results;
using Library.Domain.Sessions;
using Library.Domain.Users.Entities;
using Library.Domain.Users.Managers;
using Library.Domain.Users.Repositories;
using Library.Presentation.Authentication;
using Library.Presentation.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Library.Presentation.Controllers;

[ApiController, Route("[controller]")]
public class UsersController(
	IUsersManager userManager,
	ISessionsManager sessionsManager,
	IOptions<JwtSettings> jwtOptions,
	IUsersRepository usersRepository
) : ControllerBase
{
	[HttpPost, Route(nameof(SignUp))]
	public async Task<Result<AuthModel>> SignUp(string username, string password)
	{
		var result = await userManager.Create(username, password);
		if (result.Success) return new Result<AuthModel>(new AuthModel(result.Value, GenerateJwtToken(result.Value.Id)));
		return new(result.Message);
	}

	[HttpGet, Route(nameof(LogIn))]
	public async Task<Result<AuthModel>> LogIn(string username, string password)
	{
		var result = await userManager.LogIn(username, password);
		if (result.Success) return new Result<AuthModel>(new AuthModel(result.Value, GenerateJwtToken(result.Value.Id)));
		return new(result.Message);
	}

	[HttpGet, Route(nameof(GetAll))]
	public async Task<Result<PagedResponse<User>>> GetAll(string username, int skipCount = 0, int pageSize = int.MaxValue)
	{
		var user = await sessionsManager.GetCurrentUser();
		if (user.Type is not UserType.Admin) return new("You do not have admin rights");

		var count = await usersRepository.Count(username);
		var users = await usersRepository.GetAll(username, skipCount, pageSize);

		users.ForEach(u => u.HashedPassword = null);
		return new(new PagedResponse<User>() { Results = users, TotalCount = count });
	}


	private string GenerateJwtToken(int userId)
	{
		var jwtSettings = jwtOptions.Value;

		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(jwtSettings.SigningKey);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(
				[
					new Claim("UserId", $"{userId}")
				]
			),
			Expires = DateTime.UtcNow + jwtSettings.ExpirationDuration,
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
}
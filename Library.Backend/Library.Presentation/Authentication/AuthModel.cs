using Library.Domain.Users.Entities;

namespace Library.Presentation.Authentication;

public class AuthModel
{
	public int UserId { get; private set; }
	public string Jwt { get; private set; }
	public UserType UserType { get; private set; }
	public string Username { get; private set; }

	public AuthModel(User user, string jwt)
	{
		Jwt = jwt;
		UserId = user.Id;
		UserType = user.Type;
		Username = user.Username;
	}
}
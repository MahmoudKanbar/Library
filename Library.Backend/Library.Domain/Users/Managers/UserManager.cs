using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Library.Domain.Results;
using Library.Domain.Users.Entities;
using Library.Domain.Users.Repositories;

namespace Library.Domain.Users.Managers;

public class UserManager(IUsersRepository usersRepository) : IUsersManager
{
	public async Task<Result<User>> LogIn(string username, string password)
	{
		if (username.IsNullOrEmpty()) return new Result<User>($"{nameof(username)} cannot be null or empty");
		if (password.IsNullOrEmpty()) return new Result<User>($"{nameof(password)} cannot be null or empty");

		var user = await usersRepository.Get(username);
		if (user is null) return new Result<User>($"No user is registered by the username {username}");

		var hashedPassword = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(password)));
		if (hashedPassword != user.HashedPassword) return new Result<User>("Wrong password");

		return new Result<User>(user);
	}

	public async Task<Result<User>> Create(string username, string password, UserType type = UserType.Client)
	{
		var user = await usersRepository.Get(username);
		if (user is not null) return new Result<User>($"There is already a registered user with the username {username}");

		var newUser = await usersRepository.Insert(
			new User
			{
				Type = type,
				Username = username,
				HashedPassword = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(password)))
			}
		);

		return new Result<User>(newUser);
	}
}
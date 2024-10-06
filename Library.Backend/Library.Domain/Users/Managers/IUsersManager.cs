using Library.Domain.Results;
using Library.Domain.Users.Entities;

namespace Library.Domain.Users.Managers;

public interface IUsersManager
{
    Task<Result<User>> LogIn(string username, string password);
    Task<Result<User>> Create(string username, string password, UserType type = UserType.Client);
}
using Library.Domain.Users.Entities;

namespace Library.Domain.Users.Repositories;

public interface IUsersRepository
{
    Task<User> Get(int id);
    Task<User> Get(string username);

    Task<User> Insert(User user);
    Task<User> Update(User user);
    
    Task<long> Count(string username);
    Task<List<User>> GetAll(string username, int skipCount = 0, int pageSize = int.MaxValue);
}

using Library.Domain.Books.Entities;
using Library.Domain.Results;

namespace Library.Domain.Books.Managers;

public interface IBooksManager
{
    Task<Result<Book>> Borrow(int bookId, int userId);
    Task<Result<Book>> Release(int bookId, int userId);
    Task<Result<Book>> AddNew(string title, string author, string ISBN);
}

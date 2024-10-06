using Library.Domain.Books.Entities;

namespace Library.Domain.Books.Repositories;

public interface IBorrowingRepository
{
    Task<Borrowing> Delete(int id);
    Task<Borrowing> Insert(int bookId, int userId);

    Task<Borrowing> Get(int id);

    Task<int> Count(int? bookId, int? userId);
    Task<List<Borrowing>> GetAll(int? bookId, int? userId, bool includeUsers = false, bool includeBooks = false, int skipCount = 0, int pageSize = int.MaxValue);
}
using System;
using Library.Domain.Books.Entities;

namespace Library.Domain.Books.Repositories;

public interface IBooksRepository
{
	Task<Book> Get(int id);

	Task<Book> Update(Book book);
	Task<Book> Insert(string title, string author, string ISBN);

	Task<long> Count(string title, string author, string ISBN);
	Task<List<Book>> GetAll(string title, string author, string ISBN, int skipCount = 0, int pageSize = int.MaxValue);
}
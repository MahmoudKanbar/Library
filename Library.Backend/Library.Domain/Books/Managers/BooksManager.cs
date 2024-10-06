using Library.Domain.Books.Entities;
using Library.Domain.Books.Repositories;
using Library.Domain.Results;
using Library.Domain.Users.Repositories;

namespace Library.Domain.Books.Managers;

public class BooksManager(
	IUsersRepository usersRepository,
	IBooksRepository booksRepository,
	IBorrowingRepository borrowingRepository
) : IBooksManager
{
	public async Task<Result<Book>> AddNew(string title, string author, string ISBN)
	{
		if (ISBN.IsNullOrEmpty()) return new($"{nameof(ISBN)} cannot be null or empty");
		if (title.IsNullOrEmpty()) return new($"{nameof(title)} cannot be null or empty");
		if (author.IsNullOrEmpty()) return new($"{nameof(author)} cannot be null or empty");

		var booksThatHaveTheSameISBN = await booksRepository.GetAll(null, null, ISBN, 0, 1);
		if (booksThatHaveTheSameISBN.Count != 0) return new Result<Book>("There is already a book with the same ISBN");

		var book = await booksRepository.Insert(title, author, ISBN);
		return new Result<Book>(book);
	}

	public async Task<Result<Book>> Borrow(int bookId, int userId)
	{
		var book = await booksRepository.Get(bookId);
		var user = await usersRepository.Get(userId);

		if (user is null) return new Result<Book>("No such user");
		if (book is null) return new Result<Book>("No such book");
		if (book.Status is BookStatus.Borrowed) return new Result<Book>("The book is already borrowed");

		book.Status = BookStatus.Borrowed;

		await booksRepository.Update(book);
		await borrowingRepository.Insert(book.Id, userId);

		return new Result<Book>(book);
	}

	public async Task<Result<Book>> Release(int bookId, int userId)
	{
		var book = await booksRepository.Get(bookId);
		var user = await usersRepository.Get(userId);

		if (user is null) return new Result<Book>("No such user");
		if (book is null) return new Result<Book>("No such book");
		if (book.Status is BookStatus.Available) return new Result<Book>("The book is not borrowed");

		book.Status = BookStatus.Available;

		await booksRepository.Update(book);
		return new Result<Book>(book);
	}
}
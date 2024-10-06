using Library.Domain.Books.Entities;
using Library.Domain.Users.Entities;
using Microsoft.Data.SqlClient;

namespace Library.Infrastructure;

public static class Materializer
{
	public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

	public static User MaterializeUser(this SqlDataReader sqlDataReader)
	{
		return new User()
		{
			Id = (int)sqlDataReader[nameof(User.Id)],
			Username = sqlDataReader[nameof(User.Username)] as string,
			Type = (UserType)sqlDataReader[nameof(User.Type)],
			HashedPassword = sqlDataReader[nameof(User.HashedPassword)] as string,
		};
	}

	public static Book MaterializeBook(this SqlDataReader sqlDataReader)
	{
		return new Book()
		{
			Id = (int)sqlDataReader[nameof(Book.Id)],
			ISBN = sqlDataReader[nameof(Book.ISBN)] as string,
			Title = sqlDataReader[nameof(Book.Title)] as string,
			Author = sqlDataReader[nameof(Book.Author)] as string,
			Status = (BookStatus)sqlDataReader[nameof(Book.Status)],
		};
	}

	public static Borrowing MaterializeBorrowing(this SqlDataReader sqlDataReader, bool includeUsers = false, bool includeBooks = false)
	{
		var borrwing = new Borrowing()
		{
			Id = (long)sqlDataReader[$"{nameof(Borrowing)}_{nameof(Borrowing.Id)}"],
			BookId = (int)sqlDataReader[$"{nameof(Borrowing)}_{nameof(Borrowing.BookId)}"],
			UserId = (int)sqlDataReader[$"{nameof(Borrowing)}_{nameof(Borrowing.UserId)}"],
			CreationTime = (DateTime)sqlDataReader[$"{nameof(Borrowing)}_{nameof(Borrowing.CreationTime)}"],
		};

		if (includeUsers)
		{
			borrwing.User = new User()
			{
				Id = borrwing.UserId,
				Type = (UserType)sqlDataReader[$"{nameof(User)}_{nameof(User.Type)}"],
				Username = sqlDataReader[$"{nameof(User)}_{nameof(User.Username)}"] as string,
				HashedPassword = sqlDataReader[$"{nameof(User)}_{nameof(User.HashedPassword)}"] as string,
			};
		}

		if (includeBooks)
		{
			borrwing.Book = new Book()
			{
				Id = borrwing.BookId,
				ISBN = sqlDataReader[$"{nameof(Book)}_{nameof(Book.ISBN)}"] as string,
				Title = sqlDataReader[$"{nameof(Book)}_{nameof(Book.Title)}"] as string,
				Author = sqlDataReader[$"{nameof(Book)}_{nameof(Book.Author)}"] as string,
				Status = (BookStatus)sqlDataReader[$"{nameof(Book)}_{nameof(Book.Status)}"],
			};
		}

		return borrwing;
	}
}
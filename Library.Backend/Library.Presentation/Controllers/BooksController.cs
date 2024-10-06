using System.ComponentModel.DataAnnotations;
using Library.Domain.Books.Entities;
using Library.Domain.Books.Managers;
using Library.Domain.Books.Repositories;
using Library.Domain.Results;
using Library.Domain.Sessions;
using Library.Domain.Users.Entities;
using Library.Presentation.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Library.Presentation.Controllers;

[ApiController, Route("[controller]")]
public class BooksController(
	IBooksManager bookManager,
	IBorrowingRepository borrowingRepository,
	ISessionsManager sessionsManager,
	IBooksRepository booksRepository
) : ControllerBase
{
	[HttpPost, Route(nameof(Borrow))]
	public async Task<Result<Book>> Borrow([Required] int bookId)
	{
		var userId = sessionsManager.GetCurrentUserId();
		if (userId is null) return new("You should log-in to do that");
		return await bookManager.Borrow(bookId, userId.Value);
	}

	[HttpPost, Route(nameof(Release))]
	public async Task<Result<Book>> Release([Required] int bookId)
	{
		var userId = sessionsManager.GetCurrentUserId();
		if (userId is null) return new("You should log-in to do that");
		return await bookManager.Release(bookId, userId.Value);
	}

	[HttpPost, Route(nameof(AddNewBook))]
	public async Task<Result<Book>> AddNewBook([Required] string title, [Required] string author, [Required] string ISBN)
	{
		var user = await sessionsManager.GetCurrentUser();
		if (user is null || user.Type is not UserType.Admin) return new("You have to be an admin to do that");
		return await bookManager.AddNew(title, author, ISBN);
	}

	[HttpGet, Route(nameof(GetAll))]
	public async Task<Result<PagedResponse<Book>>> GetAll(string title, string author, string ISBN, int skipCount = 0, int pageSize = int.MaxValue)
	{
		return new(
			new PagedResponse<Book>()
			{
				TotalCount = await booksRepository.Count(title, author, ISBN),
				Results = await booksRepository.GetAll(title, author, ISBN, skipCount, pageSize),
			}
		);
	}

	[HttpGet, Route(nameof(GetAllBorrowings))]
	public async Task<Result<PagedResponse<Borrowing>>> GetAllBorrowings(int? userId, int? bookId, int skipCount = 0, int pageSize = int.MaxValue)
	{
		var user = await sessionsManager.GetCurrentUser();
		if (user.Type is not UserType.Admin) return new("You do not have admin rights");

		var count = await borrowingRepository.Count(bookId, userId);
		var borrowings = await borrowingRepository.GetAll(bookId, userId, true, true, skipCount, pageSize);

		borrowings.ForEach(b => b.User.HashedPassword = null);
		return new(new PagedResponse<Borrowing>() { Results = borrowings, TotalCount = count });
	}
}
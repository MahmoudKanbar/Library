using System.Net.Http.Headers;
using Library.WebApp.DTOs;

namespace Library.WebApp.Helpers;

public static class LibraryClient
{
	private static readonly HttpClient httpClient = new();
	public static AuthModel AuthModel { get; private set; }

	public static bool IsAdmin => AuthModel is not null && AuthModel.UserType == UserType.Admin;

	public static void LogOut() => AuthModel = null;

	public static async Task<Result<AuthModel>> LogIn(string username, string password)
	{
		var result = await SendRequest<AuthModel>(new HttpRequestMessage(HttpMethod.Get, APIs.LogIn(username, password)));

		if (result.Success)
		{
			AuthModel = result.Value;
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthModel.Jwt);
		}

		return result;
	}
	
	public static async Task<Result<Book>> AddNewBook(string title, string author, string isbn) => 
		await SendRequest<Book>(new HttpRequestMessage(HttpMethod.Post, APIs.AddNewBook(title, author, isbn)));
	
	public static async Task<Result<PagedResult<Book>>> GetBooks(string title, string author, string isbn, int skipCount, int pageSize) => 
		await SendRequest<PagedResult<Book>>(new HttpRequestMessage(HttpMethod.Get, APIs.GetBooks(title, author, isbn, skipCount, pageSize)));

	public static async Task<Result<PagedResult<User>>> GetUsers(string username, int skipCount, int pageSize) => 
		await SendRequest<PagedResult<User>>(new HttpRequestMessage(HttpMethod.Get, APIs.GetUsers(username, skipCount, pageSize)));

	public static async Task<Result<PagedResult<Borrowing>>> GetBorrowings(int? userId, int? bookId, int skipCount, int pageSize) => 
		await SendRequest<PagedResult<Borrowing>>(new HttpRequestMessage(HttpMethod.Get, APIs.GetBorrowings(userId, bookId, skipCount, pageSize)));

	public static async Task<Result<Book>> Borrow(int bookId) => 
		await SendRequest<Book>(new HttpRequestMessage(HttpMethod.Post, APIs.Borrow(bookId)));

	public static async Task<Result<Book>> Release(int bookId) => 
		await SendRequest<Book>(new HttpRequestMessage(HttpMethod.Post, APIs.Release(bookId)));

	private static async Task<Result<T>> SendRequest<T>(HttpRequestMessage httpRequestMessage)
	{
		try
		{
			var response = await httpClient.SendAsync(httpRequestMessage);
			return await response.Content.ReadFromJsonAsync<Result<T>>();
		}
		catch
		{
			return new Result<T>("Error connecting to remote server");
		}
	}
}
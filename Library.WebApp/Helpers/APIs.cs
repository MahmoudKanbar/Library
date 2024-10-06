namespace Library.WebApp.Helpers;

public static class APIs
{

    public static string BaseUrl = "http://localhost:5283";

    public static string AddNewBook(string title, string author, string isbn) =>
        Path.Combine(BaseUrl, $"Books/AddNewBook?title={title}&author={author}&isbn={isbn}");

    public static string GetBooks(string title, string author, string isbn, int skipCount, int pageSize) =>
        Path.Combine(BaseUrl, $"Books/GetAll?title={title}&author={author}&isbn={isbn}&skipCount={skipCount}&pageSize={pageSize}");

    public static string GetBorrowings(int? userId, int? bookId, int skipCount, int pageSize) =>
        Path.Combine(BaseUrl, $"Books/GetAllBorrowings?userId={userId}&bookId={bookId}&skipCount={skipCount}&pageSize={pageSize}");

    public static string Borrow(int bookId) =>
        Path.Combine(BaseUrl, $"Books/Borrow?bookId={bookId}");

    public static string Release(int bookId) =>
        Path.Combine(BaseUrl, $"Books/Release?bookId={bookId}");


    public static string LogIn(string username, string password) =>
        Path.Combine(BaseUrl, $"Users/LogIn?username={username}&password={password}");

    public static string GetUsers(string username, int skipCount, int pageSize) =>
        Path.Combine(BaseUrl, $"Users/GetAll?username={username}&skipCount={skipCount}&pageSize={pageSize}");
}

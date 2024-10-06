using System.Text;
using Library.Domain;
using Library.Domain.Books.Entities;
using Library.Domain.Books.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure.Repositories;

public class BooksRepository : IBooksRepository, IDisposable, IAsyncDisposable
{
    private readonly SqlConnection sqlConnection;

    public BooksRepository(IOptions<DbSettings> settings)
    {
        var dbSettings = settings.Value;
        var connection = $"Server={dbSettings.Server}; Database={dbSettings.Database}; User={dbSettings.User}; Password={dbSettings.Password}; TrustServerCertificate=True;";
        sqlConnection = new SqlConnection(connection);
    }

    public async Task<Book> Get(int id)
    {
	    await using var sqlCommand = new SqlCommand(
            $"SELECT * FROM {Book.TableName} as b WHERE b.{nameof(Book.Id)} = {id}",
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var result = !await sqlDataReader.ReadAsync() ? null : sqlDataReader.MaterializeBook();
        await sqlConnection.CloseAsync();
        return result;
    }

    public async Task<Book> Update(Book book)
    {
	    await using var sqlCommand = new SqlCommand(
            $"""
			 	UPDATE {Book.TableName}
			 	SET 
			 		{nameof(Book.ISBN)} = '{book.ISBN}',
			 		{nameof(Book.Title)} = '{book.Title}',
			 		{nameof(Book.Author)} = '{book.Author}',
			 		{nameof(Book.Status)} = {(int)book.Status}
			 	WHERE {nameof(Book.Id)} = {book.Id} 
			 """,
            sqlConnection
        );

        await sqlConnection.OpenAsync();

        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        if (!await sqlDataReader.ReadAsync()) book = null;

        await sqlConnection.CloseAsync();

        return book;
    }

    public async Task<Book> Insert(string title, string author, string ISBN)
    {
	    await using var sqlCommand = new SqlCommand(
            $"""
			 	INSERT INTO {Book.TableName} ({nameof(Book.Title)}, {nameof(Book.Author)}, {nameof(Book.ISBN)}, {nameof(Book.Status)})
			 	OUTPUT INSERTED.{nameof(Book.Id)}
			 	VALUES ('{title}', '{author}', '{ISBN}', {0})
			 """,
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var book = new Book()
        {
            ISBN = ISBN,
            Title = title,
            Author = author,
        };

        if (await sqlDataReader.ReadAsync()) book.Id = sqlDataReader.GetInt32(0);
        else book = null;

        await sqlConnection.CloseAsync();

        return book;
    }

    public async Task<long> Count(string title, string author, string ISBN)
    {
        var sqlSearchQueryBuilder = new StringBuilder();

        sqlSearchQueryBuilder.Append($"SELECT COUNT(1) FROM {Book.TableName} as b\n");
        if (!(ISBN.IsNullOrEmpty() && title.IsNullOrEmpty() && author.IsNullOrEmpty())) sqlSearchQueryBuilder.Append("WHERE ");

        if (!ISBN.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.ISBN)} LIKE '%{ISBN}%'\n");
        if (!ISBN.IsNullOrEmpty() && !(title.IsNullOrEmpty() && author.IsNullOrEmpty())) sqlSearchQueryBuilder.Append("AND ");

        if (!title.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.Title)} LIKE '%{title}%'\n");
        if (!title.IsNullOrEmpty() && !author.IsNullOrEmpty()) sqlSearchQueryBuilder.Append("AND ");

        if (!author.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.Author)} LIKE '%{author}%'\n");

        await using var sqlCommand = new SqlCommand(sqlSearchQueryBuilder.ToString(), sqlConnection);

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var count = await sqlDataReader.ReadAsync() ? sqlDataReader.GetSqlInt32(0).Value : 0;
        await sqlConnection.CloseAsync();
        return count;
    }

    public async Task<List<Book>> GetAll(string title, string author, string ISBN, int skipCount = 0, int pageSize = int.MaxValue)
    {
        var sqlSearchQueryBuilder = new StringBuilder();

        sqlSearchQueryBuilder.Append($"SELECT * FROM {Book.TableName} as b\n");
        if (!(ISBN.IsNullOrEmpty() && title.IsNullOrEmpty() && author.IsNullOrEmpty())) sqlSearchQueryBuilder.Append("WHERE ");

        if (!ISBN.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.ISBN)} LIKE '%{ISBN}%'\n");
        if (!ISBN.IsNullOrEmpty() && !(title.IsNullOrEmpty() && author.IsNullOrEmpty())) sqlSearchQueryBuilder.Append("AND ");

        if (!title.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.Title)} LIKE '%{title}%'\n");
        if (!title.IsNullOrEmpty() && !author.IsNullOrEmpty()) sqlSearchQueryBuilder.Append("AND ");

        if (!author.IsNullOrEmpty()) sqlSearchQueryBuilder.Append($"b.{nameof(Book.Author)} LIKE '%{author}%'\n");

        sqlSearchQueryBuilder.Append($"ORDER BY b.{nameof(Book.Id)} OFFSET {skipCount} ROWS FETCH NEXT {pageSize} ROWS ONLY\n");

        await using var sqlCommand = new SqlCommand(
            sqlSearchQueryBuilder.ToString(),
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var users = new List<Book>();
        while (await sqlDataReader.ReadAsync()) users.Add(sqlDataReader.MaterializeBook());
        await sqlConnection.CloseAsync();
        return users;
    }

    public void Dispose()
    {
	    sqlConnection?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
	    if (sqlConnection != null) await sqlConnection.DisposeAsync();
    }
}
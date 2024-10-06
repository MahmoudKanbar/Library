using System.Text;
using Library.Domain.Books.Entities;
using Library.Domain.Books.Repositories;
using Library.Domain.Users.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure.Repositories;

public class BorrowingRepository : IBorrowingRepository, IDisposable, IAsyncDisposable
{
    private readonly SqlConnection sqlConnection;

    public BorrowingRepository(IOptions<DbSettings> settings)
    {
        var dbSettings = settings.Value;
        var connection = $"Server={dbSettings.Server}; Database={dbSettings.Database}; User={dbSettings.User}; Password={dbSettings.Password}; TrustServerCertificate=True;";
        sqlConnection = new SqlConnection(connection);
    }

    public async Task<int> Count(int? bookId, int? userId)
    {
        var sqlSearchQueryBuilder = new StringBuilder();

        sqlSearchQueryBuilder.Append($"SELECT COUNT(1) FROM {Borrowing.TableName} as b\n");
        if (bookId is not null || userId is not null) sqlSearchQueryBuilder.Append("WHERE ");
        if (bookId is not null) sqlSearchQueryBuilder.Append($"b.{nameof(Borrowing.BookId)} = {bookId}\n");
        if (bookId is not null && userId is not null) sqlSearchQueryBuilder.Append("AND ");
        if (userId is not null) sqlSearchQueryBuilder.Append($"b.{nameof(Borrowing.UserId)} = {userId}\n");

        var query = sqlSearchQueryBuilder.ToString();
        await using var sqlCommand = new SqlCommand(query, sqlConnection);

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var count = await sqlDataReader.ReadAsync() ? sqlDataReader.GetSqlInt32(0).Value : 0;
        await sqlConnection.CloseAsync();
        return count;
    }

    public async Task<Borrowing> Delete(int id)
    {
        var book = await Get(id);

        await using var sqlCommand = new SqlCommand(
            $"""
			 	DELETE FROM {Borrowing.TableName} as b WHERE b.{nameof(Borrowing.Id)} = {id}
			 """,
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        await sqlConnection.CloseAsync();

        return book;
    }

    public async Task<Borrowing> Get(int id)
    {
	    await using var sqlCommand = new SqlCommand(
            $"SELECT * FROM {Borrowing.TableName} as b WHERE b.{nameof(Borrowing.Id)} = {id}",
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var result = !await sqlDataReader.ReadAsync() ? null : sqlDataReader.MaterializeBorrowing();
        await sqlConnection.CloseAsync();
        return result;
    }

    public async Task<List<Borrowing>> GetAll(int? bookId, int? userId, bool includeUsers = false, bool includeBooks = false, int skipCount = 0, int pageSize = int.MaxValue)
    {
        var sqlFieldNameBuilder = new StringBuilder();
        var sqlSearchQueryBuilder = new StringBuilder();

        sqlFieldNameBuilder.Append(
            $"""
                b.{nameof(Borrowing.Id)} AS {nameof(Borrowing)}_{nameof(Borrowing.Id)},
                b.{nameof(Borrowing.UserId)} AS {nameof(Borrowing)}_{nameof(Borrowing.UserId)},
                b.{nameof(Borrowing.BookId)} AS {nameof(Borrowing)}_{nameof(Borrowing.BookId)},
                b.{nameof(Borrowing.CreationTime)} AS {nameof(Borrowing)}_{nameof(Borrowing.CreationTime)}
            """
        );

        if (includeUsers)
        {
            sqlFieldNameBuilder.Append(",\n");
            sqlFieldNameBuilder.Append(
                $"""
                    u.{nameof(User.Type)} AS {nameof(User)}_{nameof(User.Type)},
                    u.{nameof(User.Username)} AS {nameof(User)}_{nameof(User.Username)},
                    u.{nameof(User.HashedPassword)} AS {nameof(User)}_{nameof(User.HashedPassword)}
                """
            );
        }

        if (includeBooks)
        {
            sqlFieldNameBuilder.Append(",\n");
            sqlFieldNameBuilder.Append(
                $"""
                    bk.{nameof(Book.ISBN)} AS {nameof(Book)}_{nameof(Book.ISBN)},
                    bk.{nameof(Book.Title)} AS {nameof(Book)}_{nameof(Book.Title)},
                    bk.{nameof(Book.Author)} AS {nameof(Book)}_{nameof(Book.Author)},
                    bk.{nameof(Book.Status)} AS {nameof(Book)}_{nameof(Book.Status)}
                """
            );
        }

        sqlSearchQueryBuilder.Append($"SELECT {sqlFieldNameBuilder} FROM {Borrowing.TableName} as b\n");
        
        if (includeUsers) sqlSearchQueryBuilder.Append($"LEFT JOIN {User.TableName} AS u ON u.{nameof(User.Id)} = b.{nameof(Borrowing.UserId)}\n");
        if (includeBooks) sqlSearchQueryBuilder.Append($"LEFT JOIN {Book.TableName} AS bk ON bk.{nameof(Book.Id)} = b.{nameof(Borrowing.BookId)}\n");
        
        if (bookId is not null || userId is not null) sqlSearchQueryBuilder.Append("WHERE ");
        if (bookId is not null) sqlSearchQueryBuilder.Append($"b.{nameof(Borrowing.BookId)} = {bookId}\n");
        if (bookId is not null && userId is not null) sqlSearchQueryBuilder.Append("AND ");
        if (userId is not null) sqlSearchQueryBuilder.Append($"b.{nameof(Borrowing.UserId)} = {userId}\n");
        
        sqlSearchQueryBuilder.Append($"ORDER BY b.{nameof(Borrowing.Id)} OFFSET {skipCount} ROWS FETCH NEXT {pageSize} ROWS ONLY\n");

        await using var sqlCommand = new SqlCommand(
            sqlSearchQueryBuilder.ToString(),
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var users = new List<Borrowing>();
        while (await sqlDataReader.ReadAsync()) users.Add(sqlDataReader.MaterializeBorrowing(includeUsers, includeBooks));
        await sqlConnection.CloseAsync();
        return users;
    }

    public async Task<Borrowing> Insert(int bookId, int userId)
    {
        var creationTime = DateTime.UtcNow;
        var creationTimeAsString = creationTime.ToString(Materializer.DateTimeFormat);

        await using var sqlCommand = new SqlCommand(
            $"""
			 	INSERT INTO {Borrowing.TableName} ({nameof(Borrowing.BookId)}, {nameof(Borrowing.UserId)}, {nameof(Borrowing.CreationTime)})
			 	OUTPUT INSERTED.{nameof(Borrowing.Id)}
			 	VALUES ({bookId}, {userId}, '{creationTimeAsString}')
			 """,
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var borrowing = new Borrowing()
        {
            BookId = bookId,
            UserId = userId,
            CreationTime = creationTime,
        };

        if (await sqlDataReader.ReadAsync()) borrowing.Id = sqlDataReader.GetInt64(0);
        else borrowing = null;

        await sqlConnection.CloseAsync();

        return borrowing;
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

using Library.Domain.Books.Entities;
using Library.Domain.Users.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure;

public class DatabaseCreator
{
    private readonly string databaseName;
    private readonly SqlConnection sqlConnection;

    public DatabaseCreator(IOptions<DbSettings> options)
    {
        var dbSettings = options.Value;
        databaseName = dbSettings.Database;
        var connection = $"Server={dbSettings.Server}; User={dbSettings.User}; Password={dbSettings.Password}; TrustServerCertificate=True;";
        sqlConnection = new SqlConnection(connection);
    }

    public async Task CreateDatabase()
    {
	    var createDatabaseCommand = new SqlCommand($"CREATE DATABASE {databaseName};", sqlConnection);
	    
        var createTablesCommand = new SqlCommand(
            $"""
                USE {databaseName};

                CREATE TABLE {User.TableName} (
                    {nameof(User.Id)} INT PRIMARY KEY IDENTITY(1,1),
                    {nameof(User.Username)} NVARCHAR(MAX),
                    {nameof(User.HashedPassword)} NVARCHAR(MAX),
                    {nameof(User.Type)} INT NOT NULL
                );

                CREATE TABLE {Book.TableName} (
                    {nameof(Book.Id)} INT PRIMARY KEY IDENTITY(1,1),
                    {nameof(Book.ISBN)} NVARCHAR(MAX),
                    {nameof(Book.Title)} NVARCHAR(MAX),
                    {nameof(Book.Author)} NVARCHAR(MAX),
                    {nameof(Book.Status)} INT NOT NULL,
                );

                CREATE TABLE {Borrowing.TableName} (
                    {nameof(Borrowing.Id)} BIGINT PRIMARY KEY IDENTITY(1,1),
                    {nameof(Borrowing.CreationTime)} DATETIME NOT NULL,
                    {nameof(Borrowing.UserId)} INT FOREIGN KEY REFERENCES {User.TableName}({nameof(User.Id)}),
                    {nameof(Borrowing.BookId)} INT FOREIGN KEY REFERENCES {Book.TableName}({nameof(Book.Id)}),
                );
            """,
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        await createDatabaseCommand.ExecuteNonQueryAsync();
        await createTablesCommand.ExecuteNonQueryAsync();
        await sqlConnection.CloseAsync();
    }

    public async Task<bool> DatabaseExisted()
    {
        var sqlCommand = new SqlCommand(
            $"SELECT * FROM sys.databases WHERE name = '{databaseName}'",
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var result = await sqlDataReader.ReadAsync();
        await sqlConnection.CloseAsync();
        return result;
    }
}

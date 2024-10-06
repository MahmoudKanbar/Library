using System.Data;
using Library.Domain.Users.Entities;
using Library.Domain.Users.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Library.Domain;

namespace Library.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository, IDisposable, IAsyncDisposable
{
    private readonly SqlConnection sqlConnection;

    public UsersRepository(IOptions<DbSettings> settings)
    {
        var dbSettings = settings.Value;
        var connection = $"Server={dbSettings.Server}; Database={dbSettings.Database}; User={dbSettings.User}; Password={dbSettings.Password}; TrustServerCertificate=True;";
        sqlConnection = new SqlConnection(connection);
    }

    public async Task<User> Get(int id)
    {
	    await using var sqlCommand = new SqlCommand(
            $"SELECT * FROM {User.TableName} as u WHERE u.{nameof(User.Id)} = {id}",
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var result = !await sqlDataReader.ReadAsync() ? null : sqlDataReader.MaterializeUser();
        
        await sqlConnection.CloseAsync();
        return result;
    }

    public async Task<User> Get(string username)
    {
	    await using var sqlCommand = new SqlCommand(
            $"SELECT * FROM {User.TableName} as u WHERE u.{nameof(User.Username)} = '{username}'",
            sqlConnection
        );

        await sqlConnection.OpenAsync();
        
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        var result = !await sqlDataReader.ReadAsync() ? null : sqlDataReader.MaterializeUser();
        
        await sqlConnection.CloseAsync();
        return result;
    }

    public async Task<long> Count(string username)
    {
        string sqlSearchQuery;

        if (username.IsNullOrEmpty())
        {
            sqlSearchQuery =
                $"""
				 SELECT COUNT(*) FROM {User.TableName} as u 
				 """;
        }
        else
        {
            sqlSearchQuery =
                $"""
				 SELECT COUNT(*) FROM {User.TableName} as u 
				 WHERE u.{nameof(User.Username)} LIKE '%{username}%' 
				 """;
        }

        await using var sqlCommand = new SqlCommand(sqlSearchQuery, sqlConnection);

        await sqlConnection.OpenAsync();
        
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        
        await sqlDataReader.ReadAsync();
        var count = await sqlDataReader.ReadAsync() ? sqlDataReader.GetSqlInt32(0).Value : 0;
        
        await sqlConnection.CloseAsync();
        return count;
    }

    public async Task<List<User>> GetAll(string username, int skipCount = 0, int pageSize = int.MaxValue)
    {
        string sqlSearchQuery;

        if (username.IsNullOrEmpty())
        {
            sqlSearchQuery =
                $"""
				 SELECT * FROM {User.TableName} as u 
				 ORDER BY u.{nameof(User.Id)}
				 OFFSET {skipCount} ROWS FETCH NEXT {pageSize} ROWS ONLY
				 """;
        }
        else
        {
            sqlSearchQuery =
                $"""
				 SELECT * FROM {User.TableName} as u 
				 WHERE u.{nameof(User.Username)} LIKE '%{username}%' 
				 ORDER BY u.{nameof(User.Id)}
				 OFFSET {skipCount} ROWS FETCH NEXT {pageSize} ROWS ONLY
				 """;
        }

        await using var sqlCommand = new SqlCommand(sqlSearchQuery, sqlConnection);

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var users = new List<User>();
        while (await sqlDataReader.ReadAsync()) users.Add(sqlDataReader.MaterializeUser());
        
        await sqlConnection.CloseAsync();
        return users;
    }

    public async Task<User> Insert(User user)
    {
	    await using var sqlCommand = new SqlCommand(
            $"""
			 	INSERT INTO {User.TableName} ({nameof(User.Username)}, {nameof(User.HashedPassword)}, {nameof(User.Type)}) OUTPUT INSERTED.{nameof(User.Id)}
			 	VALUES ('{user.Username}', '{user.HashedPassword}', {(int)user.Type})
			 """,
            sqlConnection
        );

        var idParameter = new SqlParameter("@InsertedId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        sqlCommand.Parameters.Add(idParameter);

        await sqlConnection.OpenAsync();
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        if (await sqlDataReader.ReadAsync()) user.Id = sqlDataReader.GetInt32(0);
        else user = null;

        await sqlConnection.CloseAsync();

        return user;
    }

    public async Task<User> Update(User user)
    {
	    await using var sqlCommand = new SqlCommand(
            $"""
			 	UPDATE {User.TableName} as u 
			 	SET 
			 		u.{nameof(User.Username)} = @{nameof(User.Username)},
			 		u.{nameof(User.HashedPassword)} = @{nameof(User.HashedPassword)})
			 	WHERE u.{nameof(User.Id)} = {user.Id} 
			 """,
            sqlConnection
        );

        sqlCommand.Parameters.AddWithValue($"@{nameof(User.HashedPassword)}", user.Username);
        sqlCommand.Parameters.AddWithValue($"@{nameof(User.HashedPassword)}", user.HashedPassword);

        await sqlConnection.OpenAsync();

        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        if (!await sqlDataReader.ReadAsync()) user = null;

        await sqlConnection.CloseAsync();

        return user;
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
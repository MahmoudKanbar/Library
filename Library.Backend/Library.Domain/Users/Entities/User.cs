namespace Library.Domain.Users.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string HashedPassword { get; set; }

    public UserType Type { get; set; }

    public const string TableName = "Users";
}

using Library.Domain.Users.Entities;

namespace Library.Domain.Books.Entities;

public class Borrowing
{
    public long Id { get; set; }

    public Book Book{ get; set; }
    public int BookId { get; set; }

    public User User{ get; set; }
    public int UserId { get; set; }


    public DateTime CreationTime { get; set; }


    public const string TableName = "Borrowings";
}
namespace Library.WebApp.DTOs;

public class Borrowing
{
    public long Id { get; set; }
    public Book Book { get; set; }
    public User User { get; set; }
    public DateTime CreationTime { get; set; }
}
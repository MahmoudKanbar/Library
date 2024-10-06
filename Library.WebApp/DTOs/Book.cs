namespace Library.WebApp.DTOs;

public class Book
{
    public int Id { get; set; }
    public string Isbn { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public BookStatus Status { get; set; }
}
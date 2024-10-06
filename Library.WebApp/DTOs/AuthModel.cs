namespace Library.WebApp.DTOs;

public class AuthModel
{
	public int UserId { get; set; }
	public string Jwt { get; set; }
	public string Username { get; set; }
	
	public UserType UserType { get; set; }
}
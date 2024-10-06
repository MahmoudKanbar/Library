namespace Library.WebApp.DTOs;

public class Result<T>
{
	public T? Value { get; set; }
	public bool Success { get; set; }
	public string? Message { get; set; }
	
	
	public Result(string message)
	{
		Success = false;
		Message = message;
	}
}
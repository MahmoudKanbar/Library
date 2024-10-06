namespace Library.Domain.Results;

public class Result<T>
{
	public T Value { get; private set; }
	public bool Success { get; private set; }
	public string Message { get; private set; }


	public Result(T value)
	{
		Value = value;
		Success = true;
	}

	public Result(string message)
	{
		Success = false;
		Message = message;
	}
}
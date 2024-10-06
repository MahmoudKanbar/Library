using System;

namespace Library.Domain.Results;


public class EmptyErrorResult(string message)
{
	public object Value => null;
	public bool Success { get; private set; } = false;
	public string Message { get; private set; } = message;
}
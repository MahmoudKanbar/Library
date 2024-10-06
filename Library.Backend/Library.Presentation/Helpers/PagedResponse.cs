namespace Library.Presentation.Helpers;

public class PagedResponse<T>
{
	public long TotalCount { get; set; }
	public List<T> Results { get; set; }
}
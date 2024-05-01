namespace TodoApi;

public class TodoItemDto
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public bool IsComplete { get; set; }

	public TodoItemDto() { }
	public TodoItemDto(Todo todo) => (Id, Name, IsComplete) = (todo.Id, todo.Name, todo.IsComplete);
}

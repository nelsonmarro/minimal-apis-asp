using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opts => opts.UseInMemoryDatabase("TodoDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
	config.DocumentName = "TodoAPI";
	config.Title = "TodoAPI v1";
	config.Version = "v1";
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseOpenApi();
	app.UseSwaggerUi(config =>
	{
		config.DocumentTitle = "TodoAPI";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}

// TodoItems Endpoints
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);

todoItems.MapGet("/complete", GetCompleteTodos);

todoItems.MapGet("/{id}", GetTodo);

todoItems.MapPost("/", CreateTodo);

todoItems.MapPut("/{id}", UpdateTodo);

todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db) =>
	TypedResults.Ok(await db.Todos.Select(t => new TodoItemDto(t)).ToListAsync());

static async Task<IResult> GetCompleteTodos(TodoDb db) =>
	TypedResults.Ok(
		await db.Todos.Where(t => t.IsComplete).Select(t => new TodoItemDto(t)).ToListAsync()
	);

static async Task<IResult> GetTodo(TodoDb db, int id) =>
	await db.Todos.FindAsync(id) is Todo todo
		? TypedResults.Ok(new TodoItemDto(todo))
		: TypedResults.NotFound();

static async Task<IResult> CreateTodo(TodoDb db, TodoItemDto todoDto)
{
	var todo = new Todo { Name = todoDto.Name, IsComplete = todoDto.IsComplete };

	db.Todos.Add(todo);
	await db.SaveChangesAsync();

	todoDto = new TodoItemDto(todo);

	return TypedResults.Created($"/todoitems/{todoDto.Id}", todoDto);
}

static async Task<IResult> UpdateTodo(TodoDb db, TodoItemDto todoDto, int id)
{
	var todoDb = await db.Todos.FindAsync(id);
	if (todoDb is null)
	{
		return TypedResults.NotFound();
	}

	todoDb.Name = todoDto.Name;
	todoDb.IsComplete = todoDto.IsComplete;

	await db.SaveChangesAsync();
	return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(TodoDb db, int id)
{
	if (await db.Todos.FindAsync(id) is Todo todo)
	{
		db.Todos.Remove(todo);
		await db.SaveChangesAsync();
		return TypedResults.NoContent();
	}

	return TypedResults.NotFound();
}

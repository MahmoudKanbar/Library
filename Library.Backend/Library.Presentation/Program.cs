using Library.Domain.Books.Managers;
using Library.Domain.Books.Repositories;
using Library.Domain.Sessions;
using Library.Domain.Users.Managers;
using Library.Domain.Users.Repositories;
using Library.Presentation.Authentication;
using Library.Infrastructure;
using Library.Infrastructure.Repositories;
using Library.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Logging.AddConsole();
builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddTransient<ISessionsManager, SessionsManager>();

builder.Services.AddTransient<IBooksRepository, BooksRepository>();
builder.Services.AddTransient<IUsersRepository, UsersRepository>();
builder.Services.AddTransient<IBorrowingRepository, BorrowingRepository>();

builder.Services.AddTransient<IUsersManager, UserManager>();
builder.Services.AddTransient<IBooksManager, BooksManager>();


builder.ConfigureJwt();

builder.Services.Configure<DbSettings>(
	builder.Configuration.GetSection(nameof(DbSettings))
);

builder.Services.Configure<JwtSettings>(
	builder.Configuration.GetSection(nameof(JwtSettings))
);

builder.Services.AddTransient<DatabaseSeeder>();
builder.Services.AddTransient<DatabaseCreator>();

var app = builder.Build();


var databaseCreator = app.Services.GetService<DatabaseCreator>();
if (!await databaseCreator.DatabaseExisted()) await databaseCreator.CreateDatabase();


var databaseSeeder = app.Services.GetService<DatabaseSeeder>();
await databaseSeeder.CreateUsers();
await databaseSeeder.CreateBooks();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandler>();

app.Run();
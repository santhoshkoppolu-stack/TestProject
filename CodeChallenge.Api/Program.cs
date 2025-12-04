using CodeChallenge.Api.Repositories;
using CodeChallenge.Api.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories
builder.Services.AddSingleton<IMessageRepository, InMemoryMessageRepository>();
builder.Services.AddScoped<IMessageLogic, MessageLogic>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionMiddleware>(); 
app.Run();

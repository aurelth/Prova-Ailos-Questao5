using Questao5.Infrastructure.Sqlite;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("DatabaseConfig"));

builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbBootstrap = scope.ServiceProvider.GetRequiredService<IDatabaseBootstrap>();
    dbBootstrap.Setup();
}

app.Run();

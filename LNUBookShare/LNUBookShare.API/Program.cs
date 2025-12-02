using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = "Host=ep-wispy-hat-adm0eu4d-pooler.c-2.us-east-1.aws.neon.tech;" +
                          "Database=neondb;" +
                          "Username=neondb_owner;" +
                          "Password=npg_GqkRolz4rhy6;" +
                          "SSL Mode=Require;" +
                          "Trust Server Certificate=true";

builder.Services.AddDbContext<LNUBookShareDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
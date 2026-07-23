using CrashDataApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CrashContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crashes.db"));

builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CrashContext>();
    CsvImporter.SeedIfEmpty(context);
}

app.UseRouting();

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Method == "OPTIONS")
    {
        ctx.Response.Headers.Append("Access-Control-Allow-Private-Network", "true");
    }
    await next();
});

app.UseCors("Angular");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crash Data API v1"));

app.UseAuthorization();
app.MapControllers().RequireCors("Angular");

app.Run();

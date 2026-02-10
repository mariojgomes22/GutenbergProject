using LibraryBackend.Data;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Service Registration
// ==========================================

// Add controllers to the container.
builder.Services.AddControllers();


// Register the Database Initializer service
builder.Services.AddScoped<DatabaseInitializer>();

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS policy to allow the frontend application
builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration.GetValue<string>("HostConnection:Url");
    var frontendPort = builder.Configuration.GetValue<int>("HostConnection:Port");
    var allowedOrigin = $"{frontendUrl}:{frontendPort}";

    options.AddPolicy("AllowAngularApp",
        builder => builder.WithOrigins(allowedOrigin)
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// ==========================================
// Database Initialization
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}

// ==========================================
// HTTP Request Pipeline Configuration
// ==========================================


app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

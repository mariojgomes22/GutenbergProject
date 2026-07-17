using LibraryBackend.Data;
using LibraryBackend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;

// Preserve raw JWT claim types (e.g. "upn", "oid") instead of remapping them
// to the legacy WS-2005 claim URIs, so User.FindFirst("upn") etc. work as expected.
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Service Registration
// ==========================================

builder.Services.AddControllers();

builder.Services.AddScoped<DatabaseInitializer>();

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();

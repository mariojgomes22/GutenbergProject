using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace LibraryBackend.Services;

/// <summary>
/// Service responsible for initializing the database schema and applying incremental updates on startup.
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=library.db";
    }

    /// <summary>
    /// Asynchronously initializes the database. 
    /// Checks if the database exists; if not, creates it. If it exists, checks for and applies updates.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting Database Initialization...");

        // Parse filename from connection string for simplicity (assuming SQLite "Data Source=...")
        var dbPath = _connectionString.Split('=')[1].Split(';')[0];
        bool dbExists = File.Exists(dbPath);

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        if (!dbExists)
        {
            _logger.LogInformation("Database file not found. Creating fresh database...");
            await CreateDatabaseAsync(connection);
        }
        else
        {
            _logger.LogInformation("Database exists. Checking for updates...");
            await UpdateDatabaseAsync(connection);
        }
        
        _logger.LogInformation("Database Initialization Completed.");
    }

    /// <summary>
    /// Creates the specific database schema from the initial script.
    /// </summary>
    /// <param name="connection">The open database connection.</param>
    private async Task CreateDatabaseAsync(IDbConnection connection)
    {
        string scriptPath = Path.Combine(AppContext.BaseDirectory, "SqlScripts", "Creation", "initial_schema.sql");
        _logger.LogInformation($"Reading creation script from: {scriptPath}");

        if (!File.Exists(scriptPath))
        {
            _logger.LogWarning($"Script not found at {scriptPath}. Trying fallback paths...");
            
            // Fallback 1: Current Directory
            string fallbackPath1 = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "Creation", "initial_schema.sql");
            if (File.Exists(fallbackPath1)) 
            {
                scriptPath = fallbackPath1;
                _logger.LogInformation($"Found script at fallback path: {scriptPath}");
            }
            else
            {
                 // Fallback 2: Development relative path (if running from bin)
                 string fallbackPath2 = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "Creation", "initial_schema.sql");
                 if (File.Exists(fallbackPath2))
                 {
                     scriptPath = fallbackPath2;
                     _logger.LogInformation($"Found script at dev fallback path: {scriptPath}");
                 }
            }
        }

        if (File.Exists(scriptPath))
        {
            string sql = await File.ReadAllTextAsync(scriptPath);
            await connection.ExecuteAsync(sql);
            _logger.LogInformation("Database created successfully with Version 1.");
        }
        else
        {
            _logger.LogError($"Creation script not found at {scriptPath} or fallbacks!");
            throw new FileNotFoundException("Could not find initial_schema.sql");
        }
    }

    /// <summary>
    /// Applies any pending SQL update scripts to the database.
    /// </summary>
    /// <param name="connection">The open database connection.</param>
    private async Task UpdateDatabaseAsync(IDbConnection connection)
    {
        // Check current version
        int currentVersion = 0;
        try 
        {
            currentVersion = await connection.ExecuteScalarAsync<int>("SELECT MAX(Version) FROM DbVersion");
        }
        catch (Exception)
        {
            // If table doesn't exist, assume version 0 or corrupted context. 
            _logger.LogWarning("DbVersion table not found or empty. Assuming Version 0.");
        }

        _logger.LogInformation($"Current Database Version: {currentVersion}");

        string updatesFolder = Path.Combine(AppContext.BaseDirectory, "SqlScripts", "Updates");
         if (!Directory.Exists(updatesFolder))
        {
            // Fallback
             updatesFolder = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "Updates");
        }

        if (!Directory.Exists(updatesFolder))
        {
             _logger.LogInformation("No Updates folder found.");
            return;
        }

        var updateFiles = Directory.GetFiles(updatesFolder, "*.sql")
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.Name) // Format expected: 2_Description.sql, 3_NextUpdate.sql
            .ToList();

        foreach (var file in updateFiles)
        {
            // Extract version from filename (e.g., "2_AddColumn.sql" -> 2)
            var fileNameParts = file.Name.Split('_');
            if (int.TryParse(fileNameParts[0], out int scriptVersion))
            {
                if (scriptVersion > currentVersion)
                {
                    _logger.LogInformation($"Applying update: {file.Name}");
                    string sql = await File.ReadAllTextAsync(file.FullName);
                    
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        await connection.ExecuteAsync(sql, transaction: transaction);
                        
                        // Update version table
                        await connection.ExecuteAsync("INSERT INTO DbVersion (Version, AppliedDate) VALUES (@Version, CURRENT_TIMESTAMP)", new { Version = scriptVersion }, transaction: transaction);
                        
                        transaction.Commit();
                        _logger.LogInformation($"Update {scriptVersion} applied successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Failed to apply update {file.Name}: {ex.Message}");
                        throw; 
                    }
                }
            }
        }
    }
}

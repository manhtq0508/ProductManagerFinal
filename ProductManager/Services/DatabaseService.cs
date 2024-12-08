using Microsoft.EntityFrameworkCore;
using ProductManager.Data;

namespace ProductManager.Services;

public class DatabaseService
{
    private AppDbContext? _appDbContext;

    public AppDbContext AppDbContext => _appDbContext ?? throw new Exception("Database is not initialized");

    public async Task InitializeDatabase(string dbPath)
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath};Foreign Keys=True")
            .Options;

        _appDbContext = new AppDbContext(dbOptions);
        await _appDbContext.Database.EnsureCreatedAsync();
    }

    public async Task DeleteDatabase()
    {
        if (_appDbContext != null)
        {
            await _appDbContext.Database.EnsureDeletedAsync();
            _appDbContext.Dispose();
            _appDbContext = null;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class Books : DbContext
{
    public DbSet<Book> Book_db { get; set; }
    public DbSet<User> User_db { get; set; }
    public string path_db { get; }

    public Books()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        path_db = System.IO.Path.Join(path, "APIBook_db.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path_db}");
}
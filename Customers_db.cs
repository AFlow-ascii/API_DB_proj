using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class Customers : DbContext
{
    public DbSet<Customer> Customers_db { get; set; }
    public string path_db { get; }

    public Customers()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        path_db = System.IO.Path.Join(path, "APICustomers.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path_db}");
}
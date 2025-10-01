// See https://aka.ms/new-console-template for more information
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
        path_db = System.IO.Path.Join(path, "Customers.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path_db}");
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.Entity<Customer>()
    //     .HasMany(e => e.Orders)
    //     .WithOne(e => e.Customer)
    //     .HasForeignKey(e => e.Customer_id)
    //     .HasPrincipalKey(e => e.Id);

    // }

}
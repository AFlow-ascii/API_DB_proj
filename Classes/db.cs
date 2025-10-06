using Microsoft.EntityFrameworkCore;

public class Users : DbContext
{
    public DbSet<Book> Book_db { get; set; }
    public DbSet<User> User_db { get; set; }
    public DbSet<Order> Order_db { get; set; }

    public string path_db { get; }

    public Users()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        path_db = System.IO.Path.Join(path, "APIBook_db.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path_db}");
    protected override void OnModelCreating(ModelBuilder modelBuilder) // this is the relation configuration of the tables
    {
        modelBuilder.Entity<User>() // user can have more orders
        .HasMany(e => e.Orders)
        .WithOne(e => e.User)
        .HasForeignKey(e => e.User_id)
        .HasPrincipalKey(e => e.Id);

        modelBuilder.Entity<Order>() // order can have more books
        .HasMany(e => e.Books)
        .WithOne(e => e.Order)
        .HasForeignKey(e => e.Order_id)
        .HasPrincipalKey(e => e.Id);
    }
}
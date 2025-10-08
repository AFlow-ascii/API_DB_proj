using Microsoft.EntityFrameworkCore;

public class Users : DbContext
{
    public DbSet<Book> Book { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Orders> Orders { get; set; }

    public string path_db { get; }

    public Users()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        path_db = System.IO.Path.Join(path, "APIBook.db");
    }

    public string GetDbScheme()
    {
        string scheme = "";
        var db_scheme = this.Model.GetEntityTypes();
        foreach (var table in db_scheme)
        {
            scheme += "table: "+table.Name+"\n";
            foreach (var column in table.GetProperties())
            {
                scheme += "column: "+column.Name+"\n";
            }
        }
        return scheme;    
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={path_db}");
    protected override void OnModelCreating(ModelBuilder modelBuilder) // this is the relation configuration of the tables
    {
        modelBuilder.Entity<User>() // user can have more Orderss
        .HasMany(e => e.Orders)
        .WithOne(e => e.User)
        .HasForeignKey(e => e.User_id)
        .HasPrincipalKey(e => e.Id);

        modelBuilder.Entity<Orders>() // Orders can have more books
        .HasMany(e => e.Books)
        .WithOne(e => e.Orders)
        .HasForeignKey(e => e.Order_id)
        .HasPrincipalKey(e => e.Id);
    }
}
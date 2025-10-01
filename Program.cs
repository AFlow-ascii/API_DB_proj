using Microsoft.EntityFrameworkCore;

// -- API SETTINGS -- 
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();

// -- DB SETTINGS --
using var db = new Customers();
Console.WriteLine($"Inserting the db in '{db.path_db}'");

// inserting the db entries
db.Add(new Customer("Alessandro", "Alessandro@gmail.com", "09/10/2007"));
db.Add(new Customer("Zheng", "Zheng@gmail.com", "26/04/2007"));
db.SaveChanges(); // saving...

// List<Product> products = new List<Product>
// {
//     new Product("Animal Farm", 10, "Book"),
//     new Product("Acer", 1000, "Laptop"),
//     new Product("Apple", 1, "Fruits")
// };

app.MapGet("/products", () => // handling GET request
{
    return Results.Ok(db.Customers_db);
})
app.MapGet("/customers", () =>)

// .WithName("getproducts")
.WithOpenApi();

app.Run();
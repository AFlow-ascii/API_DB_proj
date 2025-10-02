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
if (!db.Customers_db.Any())
{
    Console.WriteLine("Resetting the db...");
    db.Database.EnsureDeleted(); 
    db.Database.EnsureCreated();
    Console.WriteLine("Inserting the default entries...");
    db.Add(new Customer("Alessandro", "Alessandro@gmail.com", "09/10/2007"));
    db.Add(new Customer("Zheng", "Zheng@gmail.com", "26/04/2007"));
    db.SaveChanges(); // saving...
}

// -- API interactions --
string endpoint = "/customers";
app.MapGet(endpoint, () => // handling GET request
{
    return Results.Ok(db.Customers_db);
})
.WithOpenApi();
app.MapPost(endpoint, (Customer customer) => // handling POST request
{
    try
    {
        db.Add(customer);
        db.SaveChanges();
        return Results.Ok("User inserted succesfully!");
    }
    catch (Exception e)
    {
        return Results.BadRequest($"User Error! {e.ToString()}");
    }

})
.WithOpenApi();
app.MapDelete("/customers/{id:int}", (int id) => // handling DELETE request
{
    try
    {
        var find = db.Customers_db.Find(id);
        db.Customers_db.Remove(find);
        db.SaveChanges();
        return Results.Ok("User deleted succesfully!");
    }
    catch (Exception e)
    {
        return Results.BadRequest($"User Error! {e.ToString()}");
    }
})
.WithOpenApi();
app.MapPut("/customers/{id:int}", (int id, Customer customer) => // handling PUT request
{
    try
    {
        var find = db.Customers_db.Find(id);
        if (customer.Name != null)
        {
            find.Name = customer.Name;
        }
        else if (customer.Email != null)
        {
            find.Email = customer.Email;
        }
        else if (customer.DateOfBirth != null)
        {
            find.DateOfBirth = customer.DateOfBirth;
        }
        db.SaveChanges();
        return Results.Ok("User updated succesfully!");
    }
    catch (Exception e)
    {
        return Results.BadRequest($"User Error! {e.ToString()}");
    }
})
.WithOpenApi();
app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

class Program
{
    public static string secret_key = BitConverter.ToString(RandomNumberGenerator.GetBytes(256)); // Generating a secure random key for the JW tokens
    static void Main(string[] args)
    {
        // -- API SETTINGS -- 
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddSwaggerGen();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer( // this set up the JWT auth mode
            Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "crazydomain.com", 
                    ValidAudience = "crazydomain.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key)) // take use of the secret key (generated before)
                };
            }
        );
        builder.Services.AddAuthorization();


        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // -- DB SETTINGS --
        using var db = new Users(); // all the DB is a "tree" that fall from the "user" table
        Console.WriteLine($"Inserting the db in '{db.path_db}'");

        // inserting the db entries
        if (!db.User_db.Any())
        {
            DBDefault();
        }

        app.MapPost("/resetdb", () =>
        {
            DBDefault();
            return Results.Ok("DB resetted succesfully!");
        })
        .WithOpenApi();

        // -- API "customers" interactions --
        string book_endpoint = "/books";
        app.MapGet(book_endpoint, () => // handling GET request
        {
            return Results.Ok(db.Book_db);
        })
        .RequireAuthorization()
        .WithOpenApi();
        app.MapGet("/books/{id:int}", (int id) => // handling GET "id" request
        {
            try
            {
                var find = db.Book_db.Find(id);
                return Results.Ok(find);
            }
            catch (Exception e)
            {
                return Results.BadRequest($"book Error 404 not found! {e.ToString()}");
            }
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapPost(book_endpoint, (Book book) => // handling POST request
        {
            try
            {
                db.Add(book);
                db.SaveChanges();
                return Results.Ok("book inserted succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"book Error! {e.ToString()}");
            }

        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapDelete("/books/{id:int}", (int id) => // handling DELETE request
        {
            try
            {
                var find = db.Book_db.Find(id);
                db.Book_db.Remove(find);
                db.SaveChanges();
                return Results.Ok("book deleted succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"book Error! {e.ToString()}");
            }
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapPut("/books/{id:int}", (int id, Book book) => // handling PUT request
        {
            try
            {
                var find = db.Book_db.Find(id);
                if (book.Title != null)
                {
                    find.Title = book.Title;
                }
                else if (book.Autor != null)
                {
                    find.Autor = book.Autor;
                }
                else if (book.DateOfRelease != null)
                {
                    find.DateOfRelease = book.DateOfRelease;
                }
                db.SaveChanges();
                return Results.Ok("book updated succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"Customer Error! {e.ToString()}");
            }
        })
        .RequireAuthorization()
        .WithOpenApi();
        // -- end of API "customers" interactions --

        // -- API order interactions --
        string order_endpoint = "/orders";
        app.MapPost(order_endpoint, (Order order, string username) =>
        {
            var user = db.User_db.FirstOrDefault(u => u.UserName == username);
            user.Orders.Add(order);

            return Results.Ok("Order inserted succesfully!");
        })
        .RequireAuthorization()
        .WithOpenApi();
        app.MapGet(order_endpoint, () =>
        {
            var orders = db.Order_db
            .Include(o => o.Books)
            .Select(o => new // organizing the output like this to prevent endless issues
            {
                o.Id,
                Books = o.Books.Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Autor,
                    b.DateOfRelease
                }),
            });
            return Results.Ok(orders);
        })
        .RequireAuthorization()
        .WithOpenApi();
        // -- end of API "order" interactions --

        // -- API secure interactions --
        string register_endpoint = "/register";
        string login_endpoint = "/login";
        // REGISTRATION
        app.MapPost(register_endpoint, (User user) => // handling POST request
        {
            user.Password = HashSha256(user.Password);
            db.Add(user);
            db.SaveChanges();
            return Results.Ok("User inserted succesfully!");
        })
        .WithOpenApi();

        app.MapGet(register_endpoint, () => // handling GET request
        {
            var users = db.User_db
            .Include(u => u.Orders)
            .ThenInclude(o => o.Books)
            .Select(u => new // organizing the output like this to prevent endless issues 
            {
                u.Id,
                u.UserName,
                u.Password,
                Orders = u.Orders.Select(o => new
                {
                    o.Id,
                    Books = o.Books.Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Autor,
                        b.DateOfRelease
                    })
                })
            })
            .ToList();
            return Results.Ok(users);
        })
        .WithOpenApi();

        // LOGIN
        app.MapPost(login_endpoint, (User user) => // handling POST request
        {
            try
            {
                user.Password = HashSha256(user.Password);
                var found = db.User_db.Any(u => u.UserName == user.UserName && u.Password == user.Password);
                if (!found)
                {
                    throw new Exception("Invalid Password!");
                }
                var token = GenerateToken(user.UserName);
                return Results.Ok(token);
            }
            catch (Exception e)
            {
                return Results.BadRequest($"User Error! {e.ToString()}");
            }
        })
        .WithOpenApi();
        // -- end of API "secure" interactions --

        app.Run();
    }

    static string GenerateToken(string username) 
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "crazydomain.com",
            audience: "crazydomain.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    static void DBDefault() // this function completely reset the db
    {
        using var User_db = new Users();

        Console.WriteLine("Resetting the db...");
        User_db.Database.EnsureDeleted();
        User_db.Database.EnsureCreated();
        Console.WriteLine("Inserting the default entries...");
        User_db.Add(new User("Alessandro", "03-AC-67-42-16-F3-E1-5C-76-1E-E1-A5-E2-55-F0-67-95-36-23-C8-B3-88-B4-45-9E-13-F9-78-D7-C8-46-F4", new List<Order> // password is "1234"
        {
            new Order(new List<Book> {
                new Book("Animal Farm", "George Orwell", "17/08/1945"),
                new Book("1984", "George Orwell", "08/05/1949")
            }),
            new Order(new List<Book> {
                new Book("Epic Book", "Dr. Zheng", "19/12/2028")
            })
        }));
        User_db.SaveChanges(); // saving...
    }
    static string HashSha256(string s) // this convert to the hex hash with the algo "sha256"
    {
        byte[] hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return BitConverter.ToString(hashbytes);
    }
}
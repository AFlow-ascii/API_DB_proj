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
    public static string secret_key = BitConverter.ToString(RandomNumberGenerator.GetBytes(256));
    static void Main(string[] args)
    {
        // -- API SETTINGS -- 
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key))
                };
            }
        );
        builder.Services.AddAuthorization();


        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // -- DB SETTINGS --
        using var db = new Customers();
        Console.WriteLine($"Inserting the db in '{db.path_db}'");

        // inserting the db entries
        if (!db.Customers_db.Any())
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
        string custom_endpoint = "/customers";
        app.MapGet(custom_endpoint, () => // handling GET request
        {
            return Results.Ok(db.Customers_db);
        })
        .RequireAuthorization()
        .WithOpenApi();
        app.MapGet("/customers/{id:int}", (int id) => // handling GET "id" request
        {
            try
            {
                var find = db.Customers_db.Find(id);
                return Results.Ok(find);
            }
            catch (Exception e)
            {
                return Results.BadRequest($"Customer Error 404 not found! {e.ToString()}");
            }
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapPost(custom_endpoint, (Customer customer) => // handling POST request
        {
            try
            {
                db.Add(customer);
                db.SaveChanges();
                return Results.Ok("Customer inserted succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"Customer Error! {e.ToString()}");
            }

        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapDelete("/customers/{id:int}", (int id) => // handling DELETE request
        {
            try
            {
                var find = db.Customers_db.Find(id);
                db.Customers_db.Remove(find);
                db.SaveChanges();
                return Results.Ok("Customer deleted succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"Customer Error! {e.ToString()}");
            }
        })
        .RequireAuthorization()
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
                return Results.Ok("Customer updated succesfully!");
            }
            catch (Exception e)
            {
                return Results.BadRequest($"Customer Error! {e.ToString()}");
            }
        })
        .RequireAuthorization()
        .WithOpenApi();
        // -- end of API "customers" interactions --

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
            return Results.Ok(db.User_db);
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

    static void DBDefault()
    {
        using var customer_db = new Customers();

        Console.WriteLine("Resetting the db...");
        customer_db.Database.EnsureDeleted();
        customer_db.Database.EnsureCreated();
        Console.WriteLine("Inserting the default entries...");
        customer_db.Add(new Customer("Alessandro", "Alessandro@gmail.com", "09/10/2007"));
        customer_db.Add(new Customer("Zheng", "Zheng@gmail.com", "26/04/2007"));
        customer_db.SaveChanges(); // saving...
    }
    static string HashSha256(string s)
    {
        byte[] hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return BitConverter.ToString(hashbytes);
    }
}

// {"name": "Paolo", "email": "Paolo@gmail.com", "dateOfBirth": "15/07/2002"}
// {"username": "Paolo", "password": "1234"}
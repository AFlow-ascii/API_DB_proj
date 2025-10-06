using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
/*
Todo:
- Sec, insert some more secure stuffs -> controls ecc
- Reinvent in ASYNC
*/
class Program
{
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Classes.Security.jwt_secret_key)) // take use of the secret key (generated before)
                };
            }
        );
        builder.Services.AddAuthorization();


        var app = builder.Build();
        // app.UseHttpsRedirection();
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

        // Configuring API endpoints
        API.BooksApiEndpoint.setBookEndpoints(app, db);
        API.OrdersApiEndpoint.setOrderEndpoints(app, db);
        API.RegisterApiEndpoint.setRegisterEndpoints(app, db);
        API.LoginApiEndpoint.setLoginEndpoints(app, db);
    
        app.Run();
    }

    async static void DBDefault() // this function completely reset the db
    {
        using var User_db = new Users();

        Console.WriteLine("Resetting the db...");
        User_db.Database.EnsureDeleted();
        User_db.Database.EnsureCreated();
        Console.WriteLine("Inserting the default entries...");
        User_db.Add(new User("Alessandro", Classes.Security.HashArgon2("1234"), new List<Order> 
        {
            new Order(new List<Book> {
                new Book("Animal Farm", "George Orwell", "17/08/1945"),
                new Book("1984", "George Orwell", "08/05/1949")
            }),
            new Order(new List<Book> {
                new Book("Epic Book", "Dr. Zheng", "19/12/2028")
            })
        }));
        await User_db.SaveChangesAsync(); // saving...
    }
}
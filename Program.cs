using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

class Program
{

    static string basedb = "C:\\Users\\afiorini\\AppData\\Local\\APIBook.db";
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
        checkDBIntegrity(basedb);

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
        API.AIApiEndpoint.setAIEndpoints(app, db);
        app.Run();

        db.Database.GetDbConnection().Close();
        db.Dispose();

        Classes.Security.CryptDB(basedb);
    }

    async static void DBDefault() // this function completely reset the db
    {
        using var User = new Users();

        Console.WriteLine("Resetting the db...");
        User.Database.EnsureDeleted();
        User.Database.EnsureCreated();
        Console.WriteLine("Inserting the default entries...");
        User.Add(new User("Alessandro", Classes.Security.HashArgon2("1234"), new List<Orders>
        {
            new Orders(new List<Book> {
                new Book("Animal Farm", "George Orwell", "17/08/1945"),
                new Book("1984", "George Orwell", "08/05/1949")
            }),
            new Orders(new List<Book> {
                new Book("Epic Book", "Dr. Zheng", "19/12/2028")
            })
        }));
        await User.SaveChangesAsync(); // saving...
    }
    
    static void checkDBIntegrity(string basedb)
    {
        if (!File.Exists(basedb))
        {
            Console.WriteLine("Missing DB... \nCreating the DB");
            ProcessStartInfo p1 = new ProcessStartInfo("cmd.exe", "/c dotnet ef migrations add InitialCreate");
            ProcessStartInfo p2 = new ProcessStartInfo("cmd.exe", "/c dotnet ef database update");
            p1.CreateNoWindow = true;
            p2.CreateNoWindow = true;
            Process.Start(p1).WaitForExit();
            Process.Start(p2).WaitForExit();
            DBDefault();
            Console.WriteLine("DB Created");
            return;
        }

        if (File.ReadAllText(basedb) == "") // C:\Users\afiorini\AppData\Local\APIBook.db
        {
            // creating the db
            Console.WriteLine("Empty db found!");
            DBDefault();
        }
        else
        {
            // decrypt the db
            Classes.Security.DecryptDb(basedb);
        }
    }
}
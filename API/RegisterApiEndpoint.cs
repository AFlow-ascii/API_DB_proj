using Microsoft.EntityFrameworkCore;
namespace API
{
    /*
    - Sec error -> user with the same name can be registered multiple times
    */
    class RegisterApiEndpoint 
    {
        public static void setRegisterEndpoints(WebApplication app, Users db)
        {
            string register_endpoint = "/register";
            app.MapPost(register_endpoint, async (User user) => // handling POST request
            {
                if (user.UserName == null || user.Password == null)
                {
                    return Results.BadRequest("Invalid User!");
                }
                bool exist = await db.User.AnyAsync(u => u.UserName == user.UserName);
                if (exist) {
                    return Results.BadRequest("User already exist!");
                }
                Console.WriteLine($"someone who is: {user.UserName} : {user.Password} want to register");
                user.Password = Classes.Security.HashArgon2(user.Password);
                await db.AddAsync(user);
                await db.SaveChangesAsync();
                return Results.Ok("User inserted succesfully!");
            })
            .WithOpenApi();

            app.MapGet(register_endpoint, () => // handling GET request
            {
                var users = db.User
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
        }
    }
}
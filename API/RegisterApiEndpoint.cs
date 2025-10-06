using Microsoft.EntityFrameworkCore;
namespace API
{
    class RegisterApiEndpoint
    {
        public static void setRegisterEndpoints(WebApplication app, Users db)
        {
            string register_endpoint = "/register";
            app.MapPost(register_endpoint, async (User user) => // handling POST request
            {
                user.Password = Classes.Security.HashArgon2(user.Password);
                await db.AddAsync(user);
                await db.SaveChangesAsync();
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
        }
    }
}
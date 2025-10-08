using Microsoft.EntityFrameworkCore;

namespace API
{
    class LoginApiEndpoint
    {
        public static void setLoginEndpoints(WebApplication app, Users db)
        {
            string login_endpoint = "/login";
            app.MapPost(login_endpoint, async (User user) => // handling POST request
            {
                try
                {
                    user.Password = Classes.Security.HashArgon2(user.Password);

                    var found = await db.User.AnyAsync(u => u.UserName == user.UserName && u.Password == user.Password);
                    if (!found)
                    {
                        throw new Exception("Invalid Password!");
                    }
                    var token = Classes.Security.GenerateToken(user.UserName);
                    return Results.Ok(token);
                }
                catch (Exception e)
                {
                    return Results.BadRequest($"User Error! {e.ToString()}");
                }
            })
            .WithOpenApi();
        }

    }
}
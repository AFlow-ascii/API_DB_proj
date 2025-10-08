using Microsoft.EntityFrameworkCore;
namespace API
{
    class OrdersApiEndpoint
    {
        public static void setOrderEndpoints(WebApplication app, Users db)
        {
            string order_endpoint = "/orders";
            app.MapPost(order_endpoint, async (Orders order, string username) =>
            {
                var user = await db.User.FirstOrDefaultAsync(u => u.UserName == username);
                user.Orders.Add(order);

                return Results.Ok("Order inserted succesfully!");
            })
            .RequireAuthorization()
            .WithOpenApi();
            
            app.MapGet(order_endpoint, () =>
            {
                var orders = db.Orders
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
        }
    }
}
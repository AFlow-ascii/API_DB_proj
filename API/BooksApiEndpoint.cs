namespace API
{
    class BooksApiEndpoint
    {
        public static void setBookEndpoints(WebApplication app, Users db)
        {
            string book_endpoint = "/books";
            app.MapGet(book_endpoint, () => // handling GET request
            {
                Console.WriteLine("Someone want to steal the books D:");
                return Results.Ok(db.Book);
            })
            .RequireAuthorization()
            .WithOpenApi();
            app.MapGet("/books/{id:int}", (int id) => // handling GET "id" request
            {
                try
                {
                    if (db.Book.Find(id) != null)
                    {
                        return Results.Ok(db.Book.Find(id));
                    }
                    else throw new Exception("can't find book");
                }
                catch (Exception e)
                {
                    Console.WriteLine("nah bro");
                    return Results.BadRequest($"book Error 404 not found! {e.ToString()}");
                }
            })
            .RequireAuthorization()
            .WithOpenApi();

            app.MapPost(book_endpoint, async (Book book) => // handling POST request
            {
                try
                {
                    await db.AddAsync(book);
                    await db.SaveChangesAsync();
                    return Results.Ok("book inserted succesfully!");
                }
                catch (Exception e)
                {
                    return Results.BadRequest($"book Error! {e.ToString()}");
                }

            })
            .RequireAuthorization()
            .WithOpenApi();

            app.MapDelete("/books/{id:int}", async (int id) => // handling DELETE request
            {
                try
                {
                    var find = await db.Book.FindAsync(id);
                    db.Book.Remove(find);
                    await db.SaveChangesAsync();
                    return Results.Ok("book deleted succesfully!");
                }
                catch (Exception e)
                {
                    return Results.BadRequest($"book Error! {e.ToString()}");
                }
            })
            .RequireAuthorization()
            .WithOpenApi();

            app.MapPut("/books/{id:int}", async (int id, Book book) => // handling PUT request
            {
                try
                {
                    var find = await db.Book.FindAsync(id);
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
                    await db.SaveChangesAsync();
                    return Results.Ok("book updated succesfully!");
                }
                catch (Exception e)
                {
                    return Results.BadRequest($"Customer Error! {e.ToString()}");
                }
            })
            .RequireAuthorization()
            .WithOpenApi();
        }
    }
}

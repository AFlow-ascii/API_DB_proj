namespace API
{
    class BooksApiEndpoint
    {
        public static void setBookEndpoints(WebApplication app, Users db)
        {
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
        }
    }
}

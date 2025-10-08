public class Orders
{
    public int Id { get; set; } // private id
    public User User { get; set; }
    public int User_id { get; set; }
    public List<Book> Books { get; set; } = new();
    public Orders() { }
    public Orders(List<Book> books)
    {
        Books = books;
        foreach (var book in books) // this connect the order to all the books
        { 
            book.Orders = this;
        }
    }
}
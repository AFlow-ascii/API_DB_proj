public class Order
{
    public int Id { get; set; } // private id
    public User User { get; set; }
    public int User_id { get; set; }
    public List<Book> Books { get; set; } = new();
    public Order() { }
    public Order(List<Book> books)
    {
        Books = books;
        foreach (var book in books)
        {
            book.Order = this;
        }
    }
}
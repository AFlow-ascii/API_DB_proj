public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public List<Order> Orders { get; set; } = new();
    public User() { }
    public User(string username, string password, List<Order> orders)
    {
        UserName = username;
        Password = password;
        Orders = orders;
        foreach (var order in orders) // this connect the user to all the orders
        {
            order.User = this;
        }
    }
}

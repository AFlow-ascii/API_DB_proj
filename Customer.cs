public class Customer
{
    public int Id { get; set; } // private id
    public string Name { get; set; }
    public string Email { get; set; }
    public string DateOfBirth { get; set; }
    // public List<Order> Orders { get; } = new();
    public Customer() {}
    public Customer(string name, string email, string dateofbirth/*, List<Order> orders*/)
    {
        Name = name;
        Email = email;
        DateOfBirth = dateofbirth;
        // Orders = orders;
    }
}

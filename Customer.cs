public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string DateOfBirth { get; set; }
    public Customer() {}
    public Customer(string name, string email, string dateofbirth)
    {
        Name = name;
        Email = email;
        DateOfBirth = dateofbirth;
    }
}

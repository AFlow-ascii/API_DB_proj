// using Microsoft.EntityFrameworkCore;

// /*
// dotnet ef migrations add InitialCreate
// dotnet ef database update
// */

// using var db = new Customers();
// Console.WriteLine($"I've inserted the db in '{db.path_db}'");

// // creating the db
// db.Add(new Customer("Alessandro", "Alessandro@gmail.com", "09/10/2007", new List<Order>{
//     new Order("eggs", 2),
//     new Order("beef", 1)
// }));
// db.Add(new Customer("Zheng", "Zheng@gmail.com", "26/04/2007", new List<Order>{
//     new Order("pasta", 1),
//     new Order("dessert", 1)
// }));
// db.SaveChanges(); // saving...

// // reading entryes in db
// Console.WriteLine("--- TABLE 'CUSTOMER' ---");
// foreach (Customer customer in db.Customers_db)
// {
//     Console.WriteLine($"Reading entry for: {customer.Name} : '{customer.Id}'\n {customer.Email} : {customer.DateOfBirth}");
//     foreach (Order order in customer.Orders)
//     {
//         Console.WriteLine($"{customer.Name} has made order of: {order.Quantity} {order.Order_str}");
//     }
// }

// Console.WriteLine("--- ---------------- ---");
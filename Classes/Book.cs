    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Autor { get; set; }
        public string DateOfRelease { get; set; }
        public Order? Order { get; set; }
        public int? Order_id { get; set; }
        public Book() { }
        public Book(string title, string autor, string dateofrelease)
        {
            Title = title;
            Autor = autor;
            DateOfRelease = dateofrelease;
        }
    }
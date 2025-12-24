// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi




// Configure the HTTP request pipeline.








 public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
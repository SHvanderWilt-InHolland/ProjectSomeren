namespace ProjectSomeren.Models
{
    public class Drink
    {
        public int Drink_Id { get; set; }
        public string? Name { get; set; }
        public Decimal Price { get; set; }
        public int Vat { get; set; }
        public int Stock { get; set; }
        
    }
}

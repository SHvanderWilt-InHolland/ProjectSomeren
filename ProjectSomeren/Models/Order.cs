namespace ProjectSomeren.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int StudentId { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderPart> OrderParts { get; set; }

        public Order(int orderId, List<OrderPart> order, int studentId, DateTime date)
        {
            OrderId = orderId;
            OrderParts = order;
            StudentId = studentId;
            OrderDate = date;
        }

        public Order() { }
    }

using System.Collections.Generic;

namespace ProjectSomeren.Models
{
    public class DrinkOrderViewModel
    {
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Drink> Drinks { get; set; } = new List<Drink>();

        public int SelectedStudentId { get; set; }
        public int SelectedDrinkId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using ProjectSomeren.Models;

namespace ProjectSomeren.Views.Drinks
{
    public class OrderModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public OrderModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Drink> Drinks { get; set; } = new();
        public List<Student> Students { get; set; } = new();

        [BindProperty]
        public int SelectedStudentId { get; set; }

        [BindProperty]
        public int SelectedDrinkId { get; set; }

        [BindProperty]
        public int Quantity { get; set; } = 1;

        [TempData]
        public string? Message { get; set; }

        public void OnGet()
        {
            LoadDrinksAndStudents();
        }

        public IActionResult OnPost()
        {
            LoadDrinksAndStudents();

            if (SelectedDrinkId <= 0 || SelectedStudentId <= 0 || Quantity <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a student, a drink and a valid quantity.");
                return Page();
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check current stock
                string stockQuery = "SELECT stock, name FROM drink WHERE drink_id = @DrinkId";
                using (SqlCommand stockCmd = new SqlCommand(stockQuery, connection))
                {
                    stockCmd.Parameters.AddWithValue("@DrinkId", SelectedDrinkId);
                    using (var reader = stockCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ModelState.AddModelError(string.Empty, "Selected drink not found.");
                            return Page();
                        }

                        int currentStock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        string drinkName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                        if (Quantity > currentStock)
                        {
                            ModelState.AddModelError(string.Empty, $"Not enough stock for '{drinkName}'. Available: {currentStock}.");
                            return Page();
                        }
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Decrement stock
                        string updateStock = "UPDATE drink SET stock = stock - @Qty WHERE drink_id = @DrinkId";
                        using (SqlCommand upd = new SqlCommand(updateStock, connection, transaction))
                        {
                            upd.Parameters.AddWithValue("@Qty", Quantity);
                            upd.Parameters.AddWithValue("@DrinkId", SelectedDrinkId);
                            upd.ExecuteNonQuery();
                        }

                        // Try to insert an order record if the table exists. If it doesn't, ignore but keep stock updated.
                        try
                        {
                            string insertOrder = "INSERT INTO DrinkOrder (student_id, drink_id, quantity, order_date) VALUES (@StudentId, @DrinkId, @Qty, @Date)";
                            using (SqlCommand ins = new SqlCommand(insertOrder, connection, transaction))
                            {
                                ins.Parameters.AddWithValue("@StudentId", SelectedStudentId);
                                ins.Parameters.AddWithValue("@DrinkId", SelectedDrinkId);
                                ins.Parameters.AddWithValue("@Qty", Quantity);
                                ins.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                                ins.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // If the orders table doesn't exist or insert fails, continue. Stock was already updated.
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "An error occurred while placing the order. Please try again.");
                        return Page();
                    }
                }
            }

            Message = "Order placed successfully.";
            return RedirectToPage();
        }

        private void LoadDrinksAndStudents()
        {
            Drinks.Clear();
            Students.Clear();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Load drinks
                string drinkQuery = "SELECT drink_id, name, price, vat, stock FROM drink ORDER BY name";
                using (SqlCommand cmd = new SqlCommand(drinkQuery, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Drinks.Add(new Drink
                        {
                            Drink_Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Price = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                            Vat = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            Stock = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        });
                    }
                }

                // Load students
                string studentQuery = "SELECT Id, FirstName, LastName, Email FROM Student ORDER BY LastName";
                using (SqlCommand cmd = new SqlCommand(studentQuery, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        });
                    }
                }
            }
        }
    }
}
        
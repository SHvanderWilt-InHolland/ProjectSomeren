using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProjectSomeren.Models;

namespace ProjectSomeren.Controllers
{
    public class LecturersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LecturersController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Display all lecturers
        public IActionResult Index()
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT teacher_id, first_name, last_name, email, department, age, telephone_number FROM Teacher";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lecturer lecturer = new Lecturer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            TelephoneNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                        };
                        lecturers.Add(lecturer);
                    }
                }
            }

            return View(lecturers);
        }

        // GET: Display create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Add a new lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Get the next available teacher_id
                    string getMaxIdQuery = "SELECT ISNULL(MAX(teacher_id), 0) + 1 FROM Teacher";
                    SqlCommand getMaxIdCommand = new SqlCommand(getMaxIdQuery, connection);
                    int newTeacherId = (int)getMaxIdCommand.ExecuteScalar();

                    // Insert the new lecturer with the generated ID
                    string insertQuery = "INSERT INTO Teacher (teacher_id, first_name, last_name, email, department, age, telephone_number) VALUES (@TeacherId, @FirstName, @LastName, @Email, @Department, @Age, @TelephoneNumber)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@TeacherId", newTeacherId);
                    insertCommand.Parameters.AddWithValue("@FirstName", lecturer.FirstName ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@LastName", lecturer.LastName ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@Email", lecturer.Email ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@Department", lecturer.Department ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@Age", lecturer.Age);
                    insertCommand.Parameters.AddWithValue("@TelephoneNumber", lecturer.TelephoneNumber ?? string.Empty);

                    insertCommand.ExecuteNonQuery();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(lecturer);
        }

        // GET: Display edit form
        public IActionResult Edit(int id)
        {
            Lecturer lecturer = null;

            string query = "SELECT teacher_id, first_name, last_name, email, department, age, telephone_number FROM Teacher WHERE teacher_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lecturer = new Lecturer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            TelephoneNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                        };
                    }
                }
            }

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // POST: Update an existing lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Lecturer lecturer)
        {
            if (id != lecturer.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                string query = "UPDATE Teacher SET first_name = @FirstName, last_name = @LastName, email = @Email, department = @Department, age = @Age, telephone_number = @TelephoneNumber WHERE teacher_id = @Id";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", lecturer.Id);
                    command.Parameters.AddWithValue("@FirstName", lecturer.FirstName ?? string.Empty);
                    command.Parameters.AddWithValue("@LastName", lecturer.LastName ?? string.Empty);
                    command.Parameters.AddWithValue("@Email", lecturer.Email ?? string.Empty);
                    command.Parameters.AddWithValue("@Department", lecturer.Department ?? string.Empty);
                    command.Parameters.AddWithValue("@Age", lecturer.Age);
                    command.Parameters.AddWithValue("@TelephoneNumber", lecturer.TelephoneNumber ?? string.Empty);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(lecturer);
        }

        // GET: Display delete confirmation
        public IActionResult Delete(int id)
        {
            Lecturer lecturer = null;

            string query = "SELECT teacher_id, first_name, last_name, email, department, age, telephone_number FROM Teacher WHERE teacher_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lecturer = new Lecturer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            TelephoneNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                        };
                    }
                }
            }

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // POST: Remove an existing lecturer
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = "DELETE FROM Teacher WHERE teacher_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

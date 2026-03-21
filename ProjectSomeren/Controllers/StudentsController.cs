using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProjectSomeren.Models;

namespace ProjectSomeren.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public StudentsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Display all students with optional filtering by last name
        public IActionResult Index(string searchLastName)
        {
            List<Student> students = new List<Student>();

            string query = "SELECT student_id, first_name, last_name, email, enrollment_date, telephone_number FROM Student";
            
            if (!string.IsNullOrEmpty(searchLastName))
            {
                query += " WHERE last_name LIKE @SearchLastName";
            }
            
            query += " ORDER BY last_name, first_name";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                
                if (!string.IsNullOrEmpty(searchLastName))
                {
                    command.Parameters.AddWithValue("@SearchLastName", "%" + searchLastName + "%");
                }
                
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            EnrollmentDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                            TelephoneNumber = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                        students.Add(student);
                    }
                }
            }

            ViewData["SearchLastName"] = searchLastName;
            return View(students);
        }

        // GET: Display create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Add a new student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Check if a student with the same first and last name already exists
                    string checkDuplicateQuery = "SELECT COUNT(*) FROM Student WHERE first_name = @FirstName AND last_name = @LastName";
                    SqlCommand checkDuplicateCommand = new SqlCommand(checkDuplicateQuery, connection);
                    checkDuplicateCommand.Parameters.AddWithValue("@FirstName", student.FirstName ?? string.Empty);
                    checkDuplicateCommand.Parameters.AddWithValue("@LastName", student.LastName ?? string.Empty);

                    int duplicateCount = (int)checkDuplicateCommand.ExecuteScalar();

                    if (duplicateCount > 0)
                    {
                        ModelState.AddModelError(string.Empty, $"A student with the name '{student.FirstName} {student.LastName}' already exists. Please use a different name or check the existing records.");
                        return View(student);
                    }

                    // Insert the new student without specifying student_id (let SQL Server auto-generate it)
                    string insertQuery = "INSERT INTO Student (first_name, last_name, email, enrollment_date, telephone_number) VALUES (@FirstName, @LastName, @Email, @EnrollmentDate, @TelephoneNumber)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@FirstName", student.FirstName ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@LastName", student.LastName ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@Email", student.Email ?? string.Empty);
                    insertCommand.Parameters.AddWithValue("@EnrollmentDate", student.EnrollmentDate);
                    insertCommand.Parameters.AddWithValue("@TelephoneNumber", student.TelephoneNumber ?? string.Empty);

                    insertCommand.ExecuteNonQuery();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Display edit form
        public IActionResult Edit(int id)
        {
            Student student = null;

            string query = "SELECT student_id, first_name, last_name, email, enrollment_date, telephone_number FROM Student WHERE student_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            EnrollmentDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                            TelephoneNumber = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                    }
                }
            }

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Update an existing student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                string query = "UPDATE Student SET first_name = @FirstName, last_name = @LastName, email = @Email, enrollment_date = @EnrollmentDate, telephone_number = @TelephoneNumber WHERE student_id = @Id";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", student.Id);
                    command.Parameters.AddWithValue("@FirstName", student.FirstName ?? string.Empty);
                    command.Parameters.AddWithValue("@LastName", student.LastName ?? string.Empty);
                    command.Parameters.AddWithValue("@Email", student.Email ?? string.Empty);
                    command.Parameters.AddWithValue("@EnrollmentDate", student.EnrollmentDate);
                    command.Parameters.AddWithValue("@TelephoneNumber", student.TelephoneNumber ?? string.Empty);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Display delete confirmation
        public IActionResult Delete(int id)
        {
            Student student = null;

            string query = "SELECT student_id, first_name, last_name, email, enrollment_date, telephone_number FROM Student WHERE student_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            EnrollmentDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                            TelephoneNumber = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                    }
                }
            }

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Remove an existing student
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = "DELETE FROM Student WHERE student_id = @Id";

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
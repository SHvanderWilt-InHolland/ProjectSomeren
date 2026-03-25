using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProjectSomeren.Models;

namespace ProjectSomeren.Controllers
{
    public class ActivitySupervisorsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ActivitySupervisorsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Display list of all activities
        public IActionResult Index()
        {
            List<Activity> activities = GetAllActivities();
            return View(activities);
        }

        // Display activity and its supervisors
        public IActionResult Manage(int id)
        {
            Activity activity = GetActivityById(id);
            if (activity == null)
            {
                return NotFound();
            }

            List<Lecturer> currentSupervisors = GetActivitySupervisors(id);
            List<Lecturer> availableLecturers = GetAvailableLecturers(id);

            var viewModel = new ActivitySupervisorViewModel
            {
                Activity = activity,
                CurrentSupervisors = currentSupervisors,
                AvailableLecturers = availableLecturers
            };

            return View(viewModel);
        }

        // POST: Add a supervisor to an activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSupervisor(int activityId, int lecturerId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the supervisor already exists
                string checkQuery = "SELECT COUNT(*) FROM Teacher_Activity WHERE activity_id = @ActivityId AND teacher_id = @TeacherId";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@ActivityId", activityId);
                checkCommand.Parameters.AddWithValue("@TeacherId", lecturerId);

                int count = (int)checkCommand.ExecuteScalar();
                if (count == 0)
                {
                    string insertQuery = "INSERT INTO Teacher_Activity (activity_id, teacher_id) VALUES (@ActivityId, @TeacherId)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@ActivityId", activityId);
                    insertCommand.Parameters.AddWithValue("@TeacherId", lecturerId);

                    insertCommand.ExecuteNonQuery();
                }
            }

            return RedirectToAction(nameof(Manage), new { id = activityId });
        }

        // POST: Remove a supervisor from an activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSupervisor(int activityId, int lecturerId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string deleteQuery = "DELETE FROM Teacher_Activity WHERE activity_id = @ActivityId AND teacher_id = @TeacherId";
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@ActivityId", activityId);
                deleteCommand.Parameters.AddWithValue("@TeacherId", lecturerId);

                connection.Open();
                deleteCommand.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Manage), new { id = activityId });
        }

        // Helper method: Get all activities
        private List<Activity> GetAllActivities()
        {
            List<Activity> activities = new List<Activity>();

            string query = "SELECT activity_id, title, description, date FROM Activity ORDER BY title";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activities.Add(new Activity
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Date = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                        });
                    }
                }
            }

            return activities;
        }

        // Helper method: Get activity by ID
        private Activity GetActivityById(int id)
        {
            Activity activity = null;

            string query = "SELECT activity_id, title, description, date FROM Activity WHERE activity_id = @Id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        activity = new Activity
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Date = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3)
                        };
                    }
                }
            }

            return activity;
        }

        // Helper method: Get current supervisors for an activity
        private List<Lecturer> GetActivitySupervisors(int activityId)
        {
            List<Lecturer> supervisors = new List<Lecturer>();

            string query = @"SELECT t.teacher_id, t.first_name, t.last_name, t.email, t.department, t.age, t.telephone_number
                           FROM Teacher t
                           INNER JOIN Teacher_Activity a ON t.teacher_id = a.teacher_id
                           WHERE a.activity_id = @ActivityId
                           ORDER BY t.last_name, t.first_name";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ActivityId", activityId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        supervisors.Add(new Lecturer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            TelephoneNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                        });
                    }
                }
            }

            return supervisors;
        }

        // Helper method: Get lecturers not yet supervising this activity
        private List<Lecturer> GetAvailableLecturers(int activityId)
        {
            List<Lecturer> availableLecturers = new List<Lecturer>();

            string query = @"SELECT t.teacher_id, t.first_name, t.last_name, t.email, t.department, t.age, t.telephone_number
                           FROM Teacher t
                           WHERE t.teacher_id NOT IN (
                               SELECT teacher_id FROM Teacher_Activity WHERE activity_id = @ActivityId
                           )
                           ORDER BY t.last_name, t.first_name";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ActivityId", activityId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        availableLecturers.Add(new Lecturer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            TelephoneNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                        });
                    }
                }
            }

            return availableLecturers;
        }
    }
}
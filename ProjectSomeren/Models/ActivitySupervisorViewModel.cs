namespace ProjectSomeren.Models
{
    public class ActivitySupervisorViewModel
    {
        public Activity Activity { get; set; }
        public List<Lecturer> CurrentSupervisors { get; set; }
        public List<Lecturer> AvailableLecturers { get; set; }
    }
}
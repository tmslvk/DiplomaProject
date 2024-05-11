using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaProject.Model
{
    public class Lesson
    {
        public int Id { get; set; }
        public string? Day { get; set; }
        public int DayOfWeek { get;set; }
        public string? TypeOfWeek { get; set; }
        public TimeSpan? TimeOfLesson { get; set; }
        public string? PlaceOfLesson { get; set; }
        public string? Group { get; set; }
        public string? TypeOfLesson { get; set; }
        public string? Discipline { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User? User { get; set; }
        public List<BackupLessons> BackupLessons { get; set; }
        public List<Workload> Workload { get; set; }
    }
}

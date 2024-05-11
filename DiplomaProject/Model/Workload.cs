using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaProject.Model
{
    public class Workload
    {
        public int Id { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }  
        public int Hours { get; set; }
        public string Disciplenes { get; set; }
        public string TypeOfLesson { get; set; }


        [ForeignKey("LessonId")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
       
    }
}

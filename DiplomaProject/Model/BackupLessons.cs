using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaProject.Model
{
    public class BackupLessons
    {
        public int Id { get; set; }
        public Lesson Lesson { get; set; }
        public int LessonId { get; set; }
        public bool IsNew { get;set; }
        public DateTime NextLessonTime { get; set; }
        public DateTime ExpireDateTime { get; set; }
    }
}

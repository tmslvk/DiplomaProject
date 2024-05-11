namespace DiplomaProject.Model
{
    public class User
    {
        public int Id { get; set; }
        public List<Lesson?> Lesson { get; set; }
        public List<Group?> Group { get; set; }
        public List<Discipline?> Discipline { get; set; }
        public List<PlaceOfLesson?> PlaceOfLesson { get; set; }
        public string Fullname { get; set; }
        public string JobPost { get; set; }
        public string Departmant { get; set; }
        public string University { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }

    }
}

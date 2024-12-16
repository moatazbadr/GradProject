namespace Edu_plat.Model.Course_registeration
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseDescription { get; set; }
        public int Course_hours { get; set; }
        public int Course_degree { get; set; }
        public bool isRegistered { get; set; }
        public string? ApplicationUserId { get; set; }
        public int Course_level { get; set; }
        public int Course_semster { get; set; }


    }
}

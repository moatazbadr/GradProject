namespace Edu_plat.DTO.To_doDto
{
    public class ToDoDto
    {
        public int Id { get; set; } 
        public string title {  get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
        public DateTime? CreationDate  { get; set; }
    
    }
}

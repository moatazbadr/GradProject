using JWT;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
    public class TodoItems
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required, MaxLength(200)]
        public string Description { get; set; }
        public bool isDone { get; set; } = false;
        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
        [ForeignKey("ApplicationUserId")]
        public string? ApplicationUserId { get; set; }
        //public virtual ApplicationUser ApplicationUser
        //{
        //    get; set;
        //}
    }
}

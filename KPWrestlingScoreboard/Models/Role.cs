using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPWrestlingScoreboard.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [Column("IdRole")]
        public int IdRole { get; set; }
        
        [Column("RoleName")]
        public string RoleName { get; set; } = string.Empty;
        
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

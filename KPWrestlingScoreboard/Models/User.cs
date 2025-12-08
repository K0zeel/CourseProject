using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPWrestlingScoreboard.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("IdUser")]
        public int IdUser { get; set; }
        
        [Column("Login")]
        public string Login { get; set; } = string.Empty;
        
        [Column("Password")]
        public string Password { get; set; } = string.Empty;
        
        [Column("IdRole")]
        public int IdRole { get; set; }
        
        [ForeignKey("IdRole")]
        public virtual Role? Role { get; set; }
    }
}

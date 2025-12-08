using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPWrestlingScoreboard.Models
{
    [Table("Regions")]
    public class Region
    {
        [Key]
        [Column("IdRegion")]
        public int IdRegion { get; set; }
        
        [Column("RegionName")]
        public string RegionName { get; set; } = string.Empty;
        
        public virtual ICollection<Wrestler> Wrestlers { get; set; } = new List<Wrestler>();
    }
}


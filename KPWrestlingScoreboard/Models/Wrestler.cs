using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPWrestlingScoreboard.Models
{
    [Table("Wrestlers")]
    public class Wrestler
    {
        [Key]
        [Column("IdWrestler")]
        public int IdWrestler { get; set; }
        
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;
        
        [Column("BirthDate")]
        public DateTime BirthDate { get; set; }
        
        [Column("IdWeightCategory")]
        public int IdWeightCategory { get; set; }
        
        [Column("IdRegion")]
        public int IdRegion { get; set; }
        
        [ForeignKey("IdWeightCategory")]
        public virtual WeightCategory? WeightCategory { get; set; }
        
        [ForeignKey("IdRegion")]
        public virtual Region? Region { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KPWrestlingScoreboard.Models
{
    [Table("WeightCategories")]
    public class WeightCategory
    {
        [Key]
        [Column("IdWeightCategory")]
        public int IdWeightCategory { get; set; }
        
        [Column("CategoryName")]
        public int CategoryName { get; set; }  // Вес в кг
        
        public virtual ICollection<Wrestler> Wrestlers { get; set; } = new List<Wrestler>();
    }
}

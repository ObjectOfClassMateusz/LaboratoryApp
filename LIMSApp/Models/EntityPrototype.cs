using System.ComponentModel.DataAnnotations;

namespace LIMSApp.Models
{
    public class EntityPrototype
    {
        [Key]
        [Required]
        public Guid guid {  get; set; }
        public DateTime creationDate { get; set; } = DateTime.UtcNow;
        public DateTime lastModifiedDate { get; set; } = DateTime.MinValue;
    }
}

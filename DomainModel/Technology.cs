using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class Technology
    {
        public int TechnologyId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public ICollection<Resource> Resources { get; private set; } = new HashSet<Resource>();

        public override string ToString() => Name;
    }
}

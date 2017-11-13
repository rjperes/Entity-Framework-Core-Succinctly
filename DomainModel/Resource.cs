using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class Resource
    {
        public int ResourceId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public ContactInformation Contact { get; set; }
        public ICollection<ProjectResource> ProjectResources { get; private set; } = new HashSet<ProjectResource>();
        public ICollection<Technology> Technologies { get; private set; } = new HashSet<Technology>();
        public override string ToString() => Name;
    }
}

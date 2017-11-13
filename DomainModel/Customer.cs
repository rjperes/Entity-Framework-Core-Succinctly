using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class Customer
    {
        public int CustomerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ContactInformation Contact { get; set; }
        public ICollection<Project> Projects { get; private set; } = new HashSet<Project>();
        public override string ToString() => Name;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DomainModel
{
    public class Project : IValidatableObject, IAuditable
    {
        public int ProjectId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? End { get; set; }
        [Required]
        public DateTime Start { get; set; }
        public ProjectDetail Detail { get; set; }
        [Required]
        public Customer Customer { get; set; }
        public ICollection<ProjectResource> ProjectResources { get; private set; } = new HashSet<ProjectResource>();
        public IEnumerable<ProjectResource> Testers => ProjectResources.Where(x => x.Role == Role.Tester);
        public IEnumerable<ProjectResource> Developers => ProjectResources.Where(x => x.Role == Role.Developer);
        public ProjectResource ProjectManager => ProjectResources.SingleOrDefault(x => x.Role == Role.ProjectManager);

        public void AddResource(Resource resource, Role role)
        {
            resource.ProjectResources.Add(new ProjectResource()
            { Project = this, Resource = resource, Role = role });
        }

        public override string ToString() => Name;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((this.End != null) && (this.End < this.Start))
            {
                yield return new ValidationResult("End date is prior to Start date", new[] { "End" });
            }
        }
    }
}

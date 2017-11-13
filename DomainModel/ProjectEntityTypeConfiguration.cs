using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModel;

namespace DomainModel
{
    public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder
              .HasOne(x => x.Customer)
              .WithMany(x => x.Projects)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

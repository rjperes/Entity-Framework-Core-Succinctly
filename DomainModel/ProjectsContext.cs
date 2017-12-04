using System;
using System.Security.Principal;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class ProjectsContext : DbContext
    {
        public Func<String> UserProvider { get; set; } = () => WindowsIdentity.GetCurrent().Name;
        public Func<DateTime> TimestampProvider { get; set; } = () => DateTime.UtcNow;

        //public DbSet<Tool> Tools { get; private set; }
        public DbSet<Resource> Resources { get; private set; }
        public DbSet<Project> Projects { get; private set; }
        public DbSet<Customer> Customers { get; private set; }
        public DbSet<Technology> Technologies { get; private set; }

        public ProjectsContext(string connectionString) : base(GetOptions(connectionString))
        {
        }

        public ProjectsContext(DbContextOptions<ProjectsContext> options) : base(options)
        {
        }

        [DbFunction]
        public static string Soundex(string text)
        {
            throw new NotImplementedException();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            var builder = new DbContextOptionsBuilder();
            return builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("UnitTests")).Options;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CustomerManager>().HasKey(x => new { x.ResourceId, x.CustomerId });

            builder.Entity<Resource>().OwnsOne(x => x.Contact);
            builder.Entity<Customer>().OwnsOne(x => x.Contact);

            builder.Entity<Project>()
                .HasOne(x => x.Detail)
                .WithOne(x => x.Project)
                .HasForeignKey<ProjectDetail>(x => x.ProjectId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Resource>()
                .HasMany(x => x.Technologies);

            builder.Entity<Technology>()
                .HasMany(x => x.Resources);

            builder
                .Entity<Project>()
                .HasMany(x => x.ProjectResources)
                .WithOne(x => x.Project)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Project>()
                .HasOne(x => x.Detail)
                .WithOne(x => x.Project)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var entity in builder.Model.GetEntityTypes().Where(x => typeof(IAuditable).IsAssignableFrom(x.ClrType)))
            {
                entity.AddProperty("CreatedBy", typeof(string)).SetMaxLength(50);
                entity.AddProperty("CreatedAt", typeof(DateTime));
                entity.AddProperty("UpdatedBy", typeof(string)).SetMaxLength(50);
                entity.AddProperty("UpdatedAt", typeof(DateTime?));
            }

            base.OnModelCreating(builder);
        }


        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedBy").CurrentValue = UserProvider();
                        entry.Property("CreatedAt").CurrentValue = TimestampProvider();
                    }
                    else
                    {
                        entry.Property("UpdatedBy").CurrentValue = UserProvider();
                        entry.Property("UpdatedAt").CurrentValue = TimestampProvider();
                    }
                }

                Validator.ValidateObject(entry.Entity, new ValidationContext(entry.Entity));
            }

            return base.SaveChanges();
        }
    }
}

using Microsoft.EntityFrameworkCore.Design;
using DomainModel;
using Microsoft.Extensions.Configuration;

namespace UnitTests
{
    public class ProjectsContextFactory : IDesignTimeDbContextFactory<ProjectsContext>
    {
        public ProjectsContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", false);

            var configuration = configurationBuilder.Build();

            return new ProjectsContext(configuration["ConnectionStrings:Projects"]);
        }
    }
}

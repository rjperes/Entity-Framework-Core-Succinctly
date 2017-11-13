using DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class Tests
    {
        private static ProjectsContext GetContext()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", false);

            var configuration = configurationBuilder.Build();

            var connectionString = configuration["ConnectionStrings:Projects"];

            //CoreEventId
            //SqlServerEventId
            //RelationalEventId

            var loggerFactory = new LoggerFactory()
                .AddDebug((categoryName, logLevel) => (logLevel == LogLevel.Information) && (categoryName == DbLoggerCategory.Database.Command.Name))
                .AddConsole((categoryName, logLevel) => (logLevel == LogLevel.Information) && (categoryName == DbLoggerCategory.Database.Command.Name));

            var optionsBuilder = new DbContextOptionsBuilder<ProjectsContext>()
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(connectionString);

            var ctx = new ProjectsContext(optionsBuilder.Options);
            var listener = ctx.GetService<DiagnosticSource>();
            (listener as DiagnosticListener).SubscribeWithAdapter(new CommandListener());

            return ctx;
        }

        [Fact]
        public void CanGetDiagnosticEvents()
        {
            var events = new Dictionary<string, object>();
            var subscription = DiagnosticListener.AllListeners.Subscribe(listener =>
            {     
                if (listener.Name == "Microsoft.EntityFrameworkCore")
                {
                    listener.Subscribe(evt =>
                    {
                        var key = evt.Key;
                        var value = evt.Value;

                        if (key == RelationalEventId.CommandExecuting.Name)
                        {
                            //will fire event
                        }
                        if (key == RelationalEventId.CommandExecuted.Name)
                        {
                            //will fire event
                        }

                        events[key] = value;

                        Assert.True(true);
                    });
                }
            });

            using (subscription)
            using (var ctx = GetContext())
            using (ctx.Database.BeginTransaction())
            {
                ctx.Projects.ToList();
                ctx.Add(new Customer { Name = "Test", Contact = new ContactInformation { Email = "xxx@xxx", Phone = "xxx" } });
                ctx.SaveChanges();
            }
            
            Assert.NotEmpty(events);
        }

        [Fact]
        public void CanGetSql()
        {
            using (var ctx = GetContext())
            {                
                var query = ctx.Projects.Where(x => x.Start <= DateTime.Today);
                var sql = query.ToSql();

                Assert.NotNull(sql);
                Assert.NotEmpty(sql);
            }
        }

        [Fact]
        public void CanGetStateListener()
        {
            using (var ctx = GetContext())
            {
                var events = ctx.GetService<ILocalViewListener>();
                events.RegisterView((entry, state) =>
                {
                    Assert.True(true);
                });

                var customer = ctx.Customers.First();

                ctx.SaveChanges();
            }
        }

        [Fact]
        public void CanUseTransactions()
        {
            using (var ctx = GetContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                var project = new Project { Name = "A Name", Customer = ctx.Customers.Find(1), Description = "Just some project", Start = DateTime.UtcNow };
                project.Detail = new ProjectDetail { Budget = 155, Critical = false, Project = project };

                ctx.Add(project);

                ctx.SaveChanges();
            }
        }

        [Fact]
        public void CanValidate()
        {
            using (var ctx = GetContext())
            {
                var project = new Project { Name = "A Name", Customer = ctx.Customers.Find(1), Description = "Just some project", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddDays(-1) };
                project.Detail = new ProjectDetail { Budget = 155, Critical = false, Project = project };

                ctx.Add(project);

                Assert.Throws<ValidationException>(() =>
                    ctx.SaveChanges()
                );
            }
        }

        [Fact]
        public void CanCompileQueries()
        {
            var query = EF.CompileQuery<ProjectsContext, IEnumerable<Project>>(ctx => ctx.Projects.OrderBy(x => x.Name));

            using (var ctx = GetContext())
            {
                var projects = query(ctx).ToList();

                Assert.NotEmpty(projects);
            }
        }

        [Fact]
        public void CanSetShadowProperties()
        {
            using (var ctx = GetContext())
            {
                var project = new Project { Name = "A Name", Customer = ctx.Customers.Find(1), Description = "Just some project", Start = DateTime.UtcNow };
                project.Detail = new ProjectDetail { Budget = 155, Critical = false, Project = project };

                ctx.Add(project);

                var results = ctx.SaveChanges();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public void CanMixLinqAndSql()
        {
            using (var ctx = GetContext())
            {
                var projects = ctx
                    .Projects
                    .FromSql("SELECT * FROM Projects")
                    .Where(x => x.Start < DateTime.Today)
                    .ToList();

                Assert.NotEmpty(projects);
            }
        }

        [Fact]
        public void CanQueryShadowProperties()
        {
            using (var ctx = GetContext())
            {
                var todaysProjects = ctx
                    .Projects
                    .Where(x => EF.Property<DateTime>(x, "CreatedAt").Date <= DateTime.Today)
                    .ToList();

                Assert.NotEmpty(todaysProjects);
            }
        }

        [Fact]
        public void CanRetrieveProjects()
        {
            using (var ctx = GetContext())
            {
                var projects = ctx.Projects.ToList();

                Assert.NotEmpty(projects);
            }
        }

        [Fact]
        public void CanEagerLoad()
        {
            using (var ctx = GetContext())
            {
                var projectsWithDetails = ctx
                    .Projects
                    .Include(x => x.Detail)
                    .ToList();

                Assert.NotEmpty(projectsWithDetails);
                Assert.All(projectsWithDetails, x => Assert.NotNull(x.Detail));
            }
        }

        [Fact]
        public void CanGetLocalEntities()
        {
            using (var ctx = GetContext())
            {
                ctx.Projects.ToList();

                var localUnchangedEntries = ctx
                    .ChangeTracker
                    .Entries()
                    .Where(x => x.State == EntityState.Unchanged)
                    .Select(x => x.Entity)
                    .ToList();

                Assert.NotEmpty(localUnchangedEntries);
            }
        }

        [Fact]
        public void CanGetDirtyProperties()
        {
            using (var ctx = GetContext())
            {
                var customer = ctx.Customers.First();
                customer.Name += "_modified_";

                var dirtyProperties = ctx
                    .Entry(customer)
                    .Properties
                    .Where(x => x.IsModified)
                    .ToList();

                Assert.NotEmpty(dirtyProperties);
            }
        }

        [Fact]
        public void CanReloadProperty()
        {
            using (var ctx = GetContext())
            {
                var customer = ctx.Customers.First();
                var originalName = customer.Name;

                customer.Name += "_modified_";

                ctx.Entry(customer).Property(x => x.Name).EntityEntry.Reload();

                Assert.Equal(originalName, customer.Name);
            }
        }

        [Fact]
        public void CanExplicitLoadCollection()
        {
            using (var ctx = GetContext())
            {
                var customer = ctx.Customers.Where(x => x.Projects.Any()).First();

                ctx.Entry(customer).Collection(x => x.Projects).Load();

                Assert.NotEmpty(customer.Projects);
            }
        }

        [Fact]
        public void CanExplicitLoadReference()
        {
            using (var ctx = GetContext())
            {
                var project = ctx.Projects.First();

                Assert.Null(project.Detail);

                ctx.Entry(project).Reference(x => x.Detail).Load();

                Assert.NotNull(project.Detail);
            }
        }

        [Fact]
        public void CanCallDbFunction()
        {
            using (var ctx = GetContext())
            {
                var name = ctx.Customers.Select(x => ProjectsContext.Soundex(x.Name)).First();

                Assert.NotNull(name);
            }
        }

        [Fact]
        public void CanExecuteSql()
        {
            using (var ctx = GetContext())
            {
                var customers = ctx.Customers.FromSql("SELECT * FROM Customers").ToList();

                Assert.NotEmpty(customers);
            }
        }

        [Fact]
        public void CanUseLike()
        {
            using (var ctx = GetContext())
            {
                var projects = ctx.Projects.Where(x => EF.Functions.Like(x.Name, "%project")).ToList();

                Assert.NotEmpty(projects);
            }
        }

        [Fact]
        public void CanExecuteClientSideFunctions()
        {
            using (var ctx = GetContext())
            {
                var projects = ctx.Projects.Where(x => NumericExtensions.IsEven(x.Detail.Budget)).ToList();

                Assert.NotEmpty(projects);
            }
        }
    }
}

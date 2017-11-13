using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Extensions
{
    public static class DbContextExtensions
    {
        public static object GetEntityKey(this DbContext context, object entity)
        {
            var entry = context.Entry(entity);
            var keyNames = context.Model.FindEntityType(entity.GetType()).FindPrimaryKey().Properties.Select(x => x.Name);
            var keys = entry.Metadata.GetKeys();
           

            return null;
        }
    }
}

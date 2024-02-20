using System.Threading.Tasks;

namespace RazorBlog.Core.Data.Seeder;

public interface IDataSeeder
{
    /// <summary>
    /// Seed data on startup.
    /// </summary>
    /// <returns></returns>
    Task SeedData();
}

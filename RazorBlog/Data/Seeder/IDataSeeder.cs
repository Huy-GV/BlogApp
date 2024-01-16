using System.Threading.Tasks;

namespace RazorBlog.Data.Seeder;

public interface IDataSeeder
{
    /// <summary>
    /// Seed data on startup.
    /// </summary>
    /// <returns></returns>
    public Task SeedData();
}

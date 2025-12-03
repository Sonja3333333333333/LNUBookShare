using System.Threading.Tasks;

namespace LNUBookShareConsole
{
    public interface IDataSeeder
    {
        Task SeedDatabaseAsync(int recordCount);
    }
}
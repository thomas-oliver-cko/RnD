using Rnd.Core.ConsoleApp.DataGenerator.Entities;

namespace Rnd.Core.ConsoleApp
{
    static class Program
    {
        static void Main()
        {
            const string file = @"C:\Code\Marketplace\checkout-entities-api\performance\data\data.csv";
            var seeder = new EntitySeeder(file);
            seeder.SeedData().GetAwaiter().GetResult();
        }
    }
}
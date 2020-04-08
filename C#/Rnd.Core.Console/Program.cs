using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnd.Core.ConsoleApp
{
    static class Program
    {
        static async Task Main()
        {
            var path = @"C:\Code\Data\GetEntitiesByClient.csv";
            var entries = File.ReadAllLines(path).ToList();
            var header = entries[0];
            entries.RemoveAt(0);

            entries.Shuffle();

            entries = new[] {header}.Concat(entries).ToList();
            File.WriteAllLines(path, entries);
        }
    }
}

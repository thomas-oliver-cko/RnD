using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rnd.Core.ConsoleApp
{

    static class Program
    {
        static async Task Main()
        {
            var token = new CancellationTokenSource(10000).Token;
            var channel = Channel.CreateBounded<object>(new BoundedChannelOptions(100));
            await channel.Reader.ReadAsync(token);
            Console.WriteLine("done");
        }

        static async Task IndexFile()
        {
            const string basePath = @"C:\Code\Marketplace\checkout-entities-api\performance\data";

            var indexFile = Path.Combine(basePath, "indexes.csv");
            File.WriteAllText(indexFile, "");
            await using var fileWriter = File.OpenWrite(indexFile);
            await using var writer = new StreamWriter(fileWriter);
            await writer.WriteLineAsync("index,global_position,line_length");

            await using var fileReader = File.OpenRead(Path.Combine(basePath, "data.csv"));
            using var reader = new StreamReader(fileReader);
            fileReader.Position = 0;

            var index = 0;
            var position = 0;

            var writeTask = Task.Run(() => {});
            var readTask = reader.ReadLineAsync();
            var line = await readTask + Environment.NewLine;

            while (!reader.EndOfStream)
            {
                readTask = reader.ReadLineAsync();
                var length = Encoding.UTF8.GetByteCount(line);

                await writeTask;

                var indexRow = $"{index},{position},{length}";
                writeTask = writer.WriteLineAsync(indexRow);

                position += length;
                index++;
                line = await readTask + Environment.NewLine;
            }
        }
    }
}
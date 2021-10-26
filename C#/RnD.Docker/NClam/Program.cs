using nClam;
using System;
using System.IO;
using System.Linq;

namespace RnD.Docker
{
    class Program
    {
        static void Main(string[] args)
        {
            var clam = new ClamClient("localhost", 3310);
            
            using var file = File.OpenRead("./TestTextFile.txt");

            var pingResult = clam.PingAsync().Result;
            var scanResult = clam.SendAndScanFileAsync(file).Result;

            switch (scanResult.Result)
            {
                case ClamScanResults.Clean:
                    Console.WriteLine("The file is clean!");
                    break;
                case ClamScanResults.VirusDetected:
                    Console.WriteLine("Virus Found!");
                    Console.WriteLine("Virus name: {0}", scanResult.InfectedFiles.First().VirusName);
                    break;
                case ClamScanResults.Error:
                    Console.WriteLine("Woah an error occured! Error: {0}", scanResult.RawResult);
                    break;
            }
        }
    }
}

using System;
using System.Linq;

namespace Rnd.Core.Aspnet.Models
{
    public class RndModel
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string String { get; set; }
        public int Integer { get; set; }
        public long Long { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public short Short { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public sbyte Sbyte { get; set; }
        public byte Byte { get; set; }
        public bool Boolean { get; set; }
        public char Character { get; set; }
        public object Object { get; set; }

        public static RndModel GetRandom()
        {
            var rnd = new Random();
            return new RndModel
            {
                String = new string(Enumerable.Repeat(chars, rnd.Next(0, 100))
                    .Select(s => s[rnd.Next(s.Length)]).ToArray()),
                Integer = rnd.Next(int.MinValue, int.MaxValue),
                Long = rnd.Next(int.MinValue, int.MaxValue),
                Double = rnd.NextDouble(),
                Float = (float)rnd.NextDouble(),
                Short = (short)rnd.Next(short.MinValue, short.MaxValue),
                Decimal = rnd.Next(int.MinValue, int.MaxValue),
                DateTime = DateTime.UtcNow,
                Sbyte = (sbyte)rnd.Next(sbyte.MinValue, sbyte.MaxValue),
                Byte = (byte)rnd.Next(byte.MinValue, byte.MaxValue),
                Boolean = 1 == rnd.Next(0, 1),
                Character = (char)rnd.Next(char.MinValue, char.MaxValue),
                Object = new object()
            };
        }
    }
}

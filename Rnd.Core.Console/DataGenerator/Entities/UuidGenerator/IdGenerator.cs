using System;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities.UuidGenerator
{
    public static class IdGenerator
    {
        public static string GetWithPrefix(string prefix)
        {
            var bytes = Guid.NewGuid().ToByteArray();
            return prefix + Base32.Encode(bytes);
        }
    }
}
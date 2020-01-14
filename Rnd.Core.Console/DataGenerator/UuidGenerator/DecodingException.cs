using System;

namespace Rnd.Core.ConsoleApp.DataGenerator.UuidGenerator
{
    public class DecodingException : Exception
    {
        public DecodingException(string message) : base(message)
        {
        }
    }
}
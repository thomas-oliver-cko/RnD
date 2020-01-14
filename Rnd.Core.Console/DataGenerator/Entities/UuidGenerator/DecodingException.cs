using System;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities.UuidGenerator
{
    public class DecodingException : Exception
    {
        public DecodingException(string message) : base(message)
        {
        }
    }
}
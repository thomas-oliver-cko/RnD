using System;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo
{
    public static class DynamoDbJsonExtensions
    {
        public static void AddStringToDynamoDbJson(this JsonWriter writer, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(value))
                return;

            writer.WritePropertyName(name);
            writer.WriteStartObject();
            writer.WritePropertyName("S");
            writer.WriteValue(value);
            writer.WriteEndObject();
        }
    }
}

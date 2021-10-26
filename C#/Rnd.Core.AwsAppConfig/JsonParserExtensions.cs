using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Checkout.Configuration.AppConfig
{
    static class JsonParserExtensions
    {
        public static void ParseAndAdd(this IDictionary<string, string> values, JsonElement element, string? key = null)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var name = key is null ? property.Name : $"{key}:{property.Name}";
                        values.ParseAndAdd(property.Value, name);
                    }
                    break;
                case JsonValueKind.Array:
                {
                    if (string.IsNullOrEmpty(key))
                        throw new ArgumentException("Json invalid. Json must be an object");

                    values.AddArray(key!, element);
                    break;
                }
                default:
                {
                    if (string.IsNullOrEmpty(key))
                        throw new ArgumentException("Json invalid. Json must be an object");

                    values.AddValue(key!, element);
                    break;
                }
            }
        }

        static void AddArray(this IDictionary<string, string> values, string key, JsonElement value)
        {
            var count = value.GetArrayLength();

            for (var i = 0; i < count; i++)
            {
                values.ParseAndAdd(value[i], $"{key}:{i}");
            }
        }

        static void AddValue(this IDictionary<string, string> values, string key, JsonElement value)
        {
            values[key!] = value.ValueKind switch
            {
                JsonValueKind.Undefined => "null",
                JsonValueKind.Null => "null",
                JsonValueKind.Number => value.ToString() ?? "null",
                JsonValueKind.String => value.ToString() ?? "null",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.AttributeValueMapper
{
    static class ToAttributeValueExtensions
    {
        public static Dictionary<string, AttributeValue> Add<T>(this Dictionary<string, AttributeValue>? values,
            string key, T value)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            var attribute = value?.ToAttributeValue();

            if (attribute is null) return values;

            values.Add(key, attribute);
            return values;
        }

        public static AttributeValue? ToAttributeValue<T>(this T incoming) =>
            incoming switch
            {
                string value => value.ToAttributeValue(),
                DateTime value => value.ToString("O").ToAttributeValue(),
                DateTimeOffset value => value.ToString("O").ToAttributeValue(),
                AttributeValue value => value,
                IEnumerable<T> value => value.ToAttributeValue(),
                IDictionary<string, T> value => value.ToAttributeValue(),
                _ => incoming?.ToString()?.ToAttributeValue()
            };

        public static AttributeValue? ToAttributeValue(this string value)
        {
            return string.IsNullOrEmpty(value) 
                ? null 
                : new AttributeValue {S = value};
        }

        public static AttributeValue? ToAttributeValue<T>(this IEnumerable<T> values)
        {
            return values is null
                ? null
                : new AttributeValue {L = values.Select(a => a.ToAttributeValue()).ToList()};
        }

        public static AttributeValue? ToAttributeValue<T>(this IDictionary<string, T> values)
        {
            return values is null
                ? null
                : new AttributeValue {M = values.ToDictionary(s => s.Key, s => s.Value.ToAttributeValue())};
        }
    }
}

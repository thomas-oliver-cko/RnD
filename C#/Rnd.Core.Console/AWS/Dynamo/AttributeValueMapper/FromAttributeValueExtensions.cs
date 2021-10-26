using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.AttributeValueMapper
{
    public static class FromAttributeValueExtensions
    {
        public static Dictionary<string, AttributeValue>? GetAttributeValues(
            this Dictionary<string, AttributeValue>? values, string key)
        {
            if (string.IsNullOrWhiteSpace(key))  throw new ArgumentException(nameof(key));
            if (values is null) return null;

            values.TryGetValue(key, out var value);

            return value?.M;
        }
    }
}

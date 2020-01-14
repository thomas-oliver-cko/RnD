using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Dynamo.DataObject
{
    public class DynamoDbEntity : IToAttributeValues
    {
        public string? EntityId { get; set; }
        public string? ClientId { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? CountryCode { get; set; }
        public DynamoDbEntityDetails? Details { get; set; }

        public Dictionary<string, AttributeValue> ToAttributeValues()
        {
            var values = new Dictionary<string, AttributeValue>();

            values.AddAttributeValue(nameof(EntityId), EntityId);
            values.AddAttributeValue(nameof(ClientId), ClientId);
            values.AddAttributeValue(nameof(ParentId), ParentId);
            values.AddAttributeValue(nameof(Name), Name);
            values.AddAttributeValue(nameof(Type), Type);
            values.AddAttributeValue(nameof(CountryCode), CountryCode);
            values.AddAttributeValue(nameof(Details), Details);

            return values;
        }

        public static DynamoDbEntity? FromAttributeValues(Dictionary<string, AttributeValue> values)
        {
            if (values is null)
                return null;

            return new DynamoDbEntity
            {
                EntityId = values.GetString(nameof(EntityId)),
                ClientId = values.GetString(nameof(ClientId)),
                ParentId = values.GetString(nameof(ParentId)),
                Name = values.GetString(nameof(Name)),
                Type = values.GetString(nameof(Type)),
                CountryCode = values.GetString(nameof(CountryCode)),
                Details = DynamoDbEntityDetails.FromAttributeValues(values.GetAttributeValues(nameof(Details)))
            };
        }
    }

    public class DynamoDbEntityDetails : IToAttributeValues
    {
        public string? CompanyNumber { get; set; }
        public string? Industry { get; set; }
        public string? TaxNumber { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Postcode { get; set; }

        public Dictionary<string, AttributeValue> ToAttributeValues()
        {
            var values = new Dictionary<string, AttributeValue>();

            values.AddAttributeValue(nameof(CompanyNumber), CompanyNumber);
            values.AddAttributeValue(nameof(Industry), Industry);
            values.AddAttributeValue(nameof(TaxNumber), TaxNumber);
            values.AddAttributeValue(nameof(Line1), Line1);
            values.AddAttributeValue(nameof(Line2), Line2);
            values.AddAttributeValue(nameof(State), State);
            values.AddAttributeValue(nameof(City), City);
            values.AddAttributeValue(nameof(Postcode), Postcode);

            return values;
        }

        public static DynamoDbEntityDetails? FromAttributeValues(Dictionary<string, AttributeValue>? values)
        {
            if (values is null)
                return null;

            return new DynamoDbEntityDetails
            {
                CompanyNumber = values.GetString(nameof(CompanyNumber)),
                Industry = values.GetString(nameof(Industry)),
                TaxNumber = values.GetString(nameof(TaxNumber)),
                Line1 = values.GetString(nameof(Line1)),
                Line2 = values.GetString(nameof(Line2)),
                State = values.GetString(nameof(State)),
                City = values.GetString(nameof(City)),
                Postcode = values.GetString(nameof(Postcode)),
            };
        }
    }

    public interface IToAttributeValues
    {
        Dictionary<string, AttributeValue> ToAttributeValues();
    }

    public static class AttributeValueDictionaryExtensions
    {
        public static void AddAttributeValue(this Dictionary<string, AttributeValue> values, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

            if (string.IsNullOrWhiteSpace(value))
                return;

            values.Add(name, new AttributeValue{S = value});
        }

        public static void AddAttributeValue(this Dictionary<string, AttributeValue> values, string name, IToAttributeValues? value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

            if (value is null)
                return;

            values.Add(name, new AttributeValue { M = value.ToAttributeValues() });
        }

        public static string GetString(this Dictionary<string, AttributeValue> values, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            values.TryGetValue(key, out var value);

            return value?.S;
        }

        public static Dictionary<string, AttributeValue>? GetAttributeValues(this Dictionary<string, AttributeValue> values, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            values.TryGetValue(key, out var value);

            return value?.M;
        }
    }
}

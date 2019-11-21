using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.DataObject
{
    public class DynamoDbEntity
    {
        public string EntityId { get; set; }
        public string ClientId { get; set; }
        public string ParentId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string CountryCode { get; set; }
        public DynamoDbEntityDetails Details { get; set; }

        public Dictionary<string, AttributeValue> ToAttributeValues()
        {
            return new Dictionary<string, AttributeValue>
            {
                [nameof(EntityId)] = new AttributeValue { S = EntityId },
                [nameof(ClientId)] = new AttributeValue { S = ClientId },
                [nameof(ParentId)] = new AttributeValue { S = ParentId },
                [nameof(Name)] = new AttributeValue { S = Name },
                [nameof(Type)] = new AttributeValue { S = Type },
                [nameof(CountryCode)] = new AttributeValue { S = CountryCode },
                [nameof(Details)] = new AttributeValue { M = Details.ToAttributeValues() },
            };
        }

        public static DynamoDbEntity FromAttributeValues(Dictionary<string, AttributeValue> values)
        {
            return new DynamoDbEntity
            {
                EntityId = values[nameof(EntityId)].S,
                ClientId = values[nameof(ClientId)].S,
                ParentId = values[nameof(ParentId)].S,
                Name = values[nameof(Name)].S,
                Type = values[nameof(Type)].S,
                CountryCode = values[nameof(CountryCode)].S,
                Details = DynamoDbEntityDetails.FromAttributeValues(values[nameof(Details)].M)
            };
        }
    }

    public class DynamoDbEntityDetails
    {
        public string CompanyNumber { get; set; }
        public string Industry { get; set; }
        public string TaxNumber { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }

        public Dictionary<string, AttributeValue> ToAttributeValues()
        {
            return new Dictionary<string, AttributeValue>
            {
                [nameof(CompanyNumber)] = new AttributeValue { S = CompanyNumber },
                [nameof(Industry)] = new AttributeValue { S = Industry },
                [nameof(TaxNumber)] = new AttributeValue { S = TaxNumber },
                [nameof(Line1)] = new AttributeValue { S = Line1 },
                [nameof(Line2)] = new AttributeValue { S = Line2 },
                [nameof(State)] = new AttributeValue { S = State },
                [nameof(City)] = new AttributeValue { S = City },
                [nameof(Postcode)] = new AttributeValue { S = Postcode },
            };
        }

        public static DynamoDbEntityDetails FromAttributeValues(Dictionary<string, AttributeValue> values)
        {
            return new DynamoDbEntityDetails
            {
                CompanyNumber = values[nameof(CompanyNumber)].S,
                Industry = values[nameof(Industry)].S,
                TaxNumber = values[nameof(TaxNumber)].S,
                Line1 = values[nameof(Line1)].S,
                Line2 = values[nameof(Line2)].S,
                State = values[nameof(State)].S,
                City = values[nameof(City)].S,
                Postcode = values[nameof(Postcode)].S,
            };
        }
    }
}

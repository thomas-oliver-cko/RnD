using Newtonsoft.Json;
using Rnd.Core.ConsoleApp.AWS.Dynamo;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities
{

    public class Entity : IToDynamoJson
    {
        public string? EntityId { get; set; }
        public string? ClientId { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public EntityType Type { get; set; }
        public string? CountryCode { get; set; }
        public EntityDetails? Details { get; set; }

        public void Write(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.AddStringToDynamoDbJson(nameof(ClientId), ClientId);
            writer.AddStringToDynamoDbJson(nameof(ParentId), ParentId);
            writer.AddStringToDynamoDbJson(nameof(EntityId), EntityId);
            writer.AddStringToDynamoDbJson(nameof(Name), Name);
            writer.AddStringToDynamoDbJson(nameof(Type), Type.ToString());
            writer.AddStringToDynamoDbJson(nameof(CountryCode), CountryCode);

            if (Details != null)
            {
                writer.WritePropertyName(nameof(Details));
                writer.WriteStartObject();
                writer.WritePropertyName("M");
                writer.WriteStartObject();
                writer.AddStringToDynamoDbJson(nameof(Details.CompanyNumber), Details.CompanyNumber);
                writer.AddStringToDynamoDbJson(nameof(Details.Industry), Details.Industry);
                writer.AddStringToDynamoDbJson(nameof(Details.TaxNumber), Details.TaxNumber);
                writer.AddStringToDynamoDbJson(nameof(Details.Line1), Details.Line1);
                writer.AddStringToDynamoDbJson(nameof(Details.Line2), Details.Line2);
                writer.AddStringToDynamoDbJson(nameof(Details.State), Details.State);
                writer.AddStringToDynamoDbJson(nameof(Details.City), Details.City);
                writer.AddStringToDynamoDbJson(nameof(Details.Postcode), Details.Postcode);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }

    public enum EntityType
    {
        Legal
    }

    public class EntityDetails
    {
        public string? CompanyNumber { get; set; }
        public string? Industry { get; set; }
        public string? TaxNumber { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Postcode { get; set; }
    }

    public class Client
    {
        public string ClientId { get; set; }
        public int EntityCount { get; set; }
        public string Entities { get; set; }
    }
}

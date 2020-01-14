﻿using Bogus;
using Bogus.DataSets;
using Rnd.Core.ConsoleApp.DataGenerator.UuidGenerator;

namespace Rnd.Core.ConsoleApp.DataGenerator
{
    class EntityGenerator : TreeGenerator<Entity>
    {
        readonly Faker faker = new Faker();

        protected override Entity CreateRoot()
        {
            return new Entity
            {
                ClientId = IdGenerator.GetWithPrefix("cli_"),
            };
        }

        protected override Entity Create(Entity rootElement, Entity? parent)
        {
            return new Entity
            {
                EntityId = IdGenerator.GetWithPrefix("ent_"),
                ClientId = rootElement.ClientId,
                ParentId = parent?.EntityId,
                Name = faker.Company.CompanyName(),
                Type = EntityType.Legal,
                CountryCode = faker.Address.CountryCode(Iso3166Format.Alpha3),
                Details = new EntityDetails
                {
                    City = faker.Address.City(),
                    CompanyNumber = faker.Random.AlphaNumeric(10),
                    Industry = faker.Commerce.Product(),
                    Line1 = faker.Address.SecondaryAddress(),
                    Line2 = faker.Address.BuildingNumber() + " " + faker.Address.StreetName(),
                    State = faker.Address.City(),
                    Postcode = faker.Address.ZipCode(),
                    TaxNumber = faker.Random.AlphaNumeric(10)
                }
            };
        }
    }

    public class Entity
    {
        public string? EntityId { get; set; }
        public string? ClientId { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public EntityType Type { get; set; }
        public string? CountryCode { get; set; }
        public EntityDetails? Details { get; set; }
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
}
using Bogus;
using Bogus.DataSets;
using Rnd.Core.ConsoleApp.DataGenerator.Entities.UuidGenerator;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities
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
}

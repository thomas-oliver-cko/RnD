using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Bogus.DataSets;

namespace Rnd.Core.ConsoleApp.DataGenerator
{
    class Generator
    {
        readonly Faker faker = new Faker();

        public List<Entity> CreateEntities(int clientCount, int entityCount, int depthCount)
        {
            var entities = new List<Entity>(entityCount + 2);

            var clientDistribution = Enumerable.Range(0, clientCount).Select(s => faker.Random.Double(1, 100)).ToList();
            var clientDistributionNormalisationParameter = clientDistribution.Sum();
            clientDistribution = clientDistribution.Select(s => s / clientDistributionNormalisationParameter).ToList();

            for (var i = 0; i < clientCount; i++)
            {
                var clientId = Guid.NewGuid();

                var entityCountInClient = Math.Round(entityCount * clientDistribution[i]);

                var entityLevelDistribution = Enumerable.Range(0, depthCount).Select(s => faker.Random.Double(1, 100)).ToList();
                var entityLevelDistributionNormalisationParameter = entityLevelDistribution.Sum();
                entityLevelDistribution = entityLevelDistribution.Select(s => s / entityLevelDistributionNormalisationParameter).ToList();
                
                var entitiesInPreviousLevel = new List<Entity>();
                var entitiesInLevel = new List<Entity>();

                for (var j = 0; j < depthCount; j++)
                {
                    var entityCountInLevel = Math.Round(entityCountInClient * entityLevelDistribution[j]);

                    entitiesInPreviousLevel = entitiesInLevel;
                    entitiesInLevel = new List<Entity>();

                    for (var k = 0; k < entityCountInLevel; k++)
                    {
                        if (!entitiesInPreviousLevel.Any())
                        {
                            var entity = CreateEntity(clientId, null);
                            entitiesInLevel.Add(entity);
                            entities.Add(entity);
                        }

                        else
                        {
                            var parentEntityIndex = faker.Random.Number(0, entitiesInPreviousLevel.Count - 1);
                            var parentEntity = entitiesInPreviousLevel[parentEntityIndex];
                            var entity = CreateEntity(clientId, parentEntity.EntityId);
                            entitiesInLevel.Add(entity);
                            entities.Add(entity);
                        }
                    }
                }
            }

            return entities;
        }

        public Entity CreateEntity(Guid clientId, Guid? parentId)
        {

            return new Entity
            {
                EntityId = Guid.NewGuid(),
                ClientId = clientId,
                ParentId = parentId,
                Name = faker.Company.CompanyName(),
                Type = EntityType.Legal,
                CountryCode = faker.Address.CountryCode(Iso3166Format.Alpha3),
                Details = new Details
                {
                    City = faker.Address.City(),
                    CompanyNumber = faker.Random.AlphaNumeric(10),
                    Industry = faker.Commerce.Product(),
                    Line1 = faker.Address.SecondaryAddress(),
                    Line2 = faker.Address.BuildingNumber() + " " + faker.Address.StreetName(),
                    State = faker.Address.City(),
                    Postcode = faker.Address.ZipCode(),
                    TaxNumber = faker.Random.Number()
                }
            };
        }
    }

    public class Entity
    {
        public Guid? EntityId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public EntityType Type { get; set; }
        public string CountryCode { get; set; }
        public Details Details { get; set; }
    }

    public enum EntityType
    {
        Legal
    }

    public class Details
    {
        public string CompanyNumber { get; set; }
        public string Industry { get; set; }
        public int TaxNumber { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
    }
}

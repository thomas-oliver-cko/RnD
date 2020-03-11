using System;
using System.Linq;
using Rnd.Core.Lambda.DynamoDb.Models;

namespace Rnd.Core.Lambda.DynamoDb
{
    class CsvEntity
    {
        public string? EntityId { get; set; }
        public string? ClientId { get; set; }
        public string? ParentId { get; set; }
        public string Name { get; set; }
        public EntityType Type { get; set; }
        public string? CkoLegalEntity { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public string? PrincipalLine1 { get; set; }
        public string? PrincipalLine2 { get; set; }
        public string? PrincipalState { get; set; }
        public string? PrincipalCity { get; set; }
        public string? PrincipalPostalCode { get; set; }
        public string? PrincipalCountryName { get; set; }
        public string? PrincipalCountryIso3Code { get; set; }

        public string? RegisteredLine1 { get; set; }
        public string? RegisteredLine2 { get; set; }
        public string? RegisteredState { get; set; }
        public string? RegisteredCity { get; set; }
        public string? RegisteredPostalCode { get; set; }
        public string? RegisteredCountryName { get; set; }
        public string? RegisteredCountryIso3Code { get; set; }

        public Details Details { get; set; }

        public static CsvEntity Get(Entity entity)
        {
            var registered = entity.Addresses.FirstOrDefault(s => s?.Type == AddressType.Registered);
            var principal = entity.Addresses.FirstOrDefault(s => s?.Type == AddressType.Principal);

            return new CsvEntity
            {
                ClientId = entity.ClientId,
                CkoLegalEntity = entity.CkoLegalEntity,
                DateModified = entity.DateModified,
                DateCreated = entity.DateCreated,
                Details = entity.Details,
                EntityId = entity.EntityId,
                Name = entity.Name,
                ParentId = entity.ParentId,
                PrincipalPostalCode = principal?.PostalCode,
                PrincipalCity  = principal?.City,
                PrincipalCountryName = principal?.Country?.Name,
                PrincipalCountryIso3Code = principal?.Country?.Iso3Code,
                PrincipalLine1 = principal?.Line1,
                PrincipalLine2 = principal?.Line2,
                PrincipalState = principal?.State,
                RegisteredPostalCode = registered?.PostalCode,
                RegisteredCountryName = registered?.Country?.Name,
                RegisteredCountryIso3Code = registered?.Country?.Iso3Code,
                RegisteredCity = registered?.City,
                RegisteredLine1 = registered?.Line1,
                RegisteredLine2 = registered?.Line2,
                RegisteredState = registered?.State,
                Type = entity.Type,
            };
        }

        public Entity ToEntity()
        {
            return new Entity
            {
                EntityId = EntityId,
                Details = Details,
                Name = Name, Type = Type,
                ClientId = ClientId, 
                CkoLegalEntity = CkoLegalEntity,
                DateCreated = DateCreated,
                Addresses = new []
                {
                    new Address
                    {
                        Type = AddressType.Principal,
                        City = PrincipalCity,
                        State = PrincipalState,
                        Line1 = PrincipalLine1,
                        Line2 = PrincipalLine2,
                        Country = new Country
                        {
                            Iso3Code = PrincipalCountryIso3Code,
                            Name = PrincipalCountryName 
                        },
                        PostalCode = PrincipalPostalCode
                    }, new Address
                    {
                        Type = AddressType.Registered,
                        City = RegisteredCity,
                        State = RegisteredState,
                        Line1 = RegisteredLine1,
                        Line2 = RegisteredLine2,
                        Country = new Country
                        {
                            Iso3Code = RegisteredCountryIso3Code,
                            Name = RegisteredCountryName
                        },
                        PostalCode = RegisteredPostalCode
                    }
                },
                DateModified = DateModified,
                ParentId = ParentId
            };
        }
    }
}

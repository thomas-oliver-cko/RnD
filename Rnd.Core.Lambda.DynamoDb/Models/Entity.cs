using System;
using System.Collections.Generic;

namespace Rnd.Core.Lambda.DynamoDb.Models
{
    public class Entity
    {
        public string EntityId { get; set; }
        public string ClientId { get; set; }
        public string? ParentId { get; set; }
        public string Name { get; set; }
        public EntityType Type { get; set; }
        public string CkoLegalEntity { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public IReadOnlyCollection<Address> Addresses { get; set; }
        public Details Details { get; set; }
    }
}
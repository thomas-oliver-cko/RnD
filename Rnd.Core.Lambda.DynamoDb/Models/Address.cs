using System;

namespace Rnd.Core.Lambda.DynamoDb.Models
{
    public class Address
    {
        public AddressType Type { get; set; }
        public string Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public Country Country { get; set; }
    }
}
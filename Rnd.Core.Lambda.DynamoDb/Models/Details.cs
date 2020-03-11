namespace Rnd.Core.Lambda.DynamoDb.Models
{
    public class Details
    {
        public string? CompanyNumber { get; set; }

        public string? Industry { get; set; }

        public string TaxNumber { get; set; }

        public Details(
            string? companyNumber, 
            string? industry, 
            string taxNumber)
        {
            CompanyNumber = companyNumber;
            Industry = industry;
            TaxNumber = taxNumber;
        }
    }
}
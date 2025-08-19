using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class FareAttribute
    {
        [Name("agency_id")]
        public string? AgencyId { get; set; }
        
        [Name("fare_id")]
        public string? FareId { get; set; }
        
        [Name("price")]
        public decimal? Price { get; set; }
        
        [Name("currency_type")]
        public string? CurrencyType { get; set; }
        
        [Name("payment_method")]
        public int? PaymentMethod { get; set; }
        
        [Name("transfers")]
        public int? Transfers { get; set; }
        
        [Name("transfer_duration")]
        public string? TransferDuration { get; set; }
    }
}

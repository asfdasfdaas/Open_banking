using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class DepositProductResponse
    {
        public DepositDataSection Data { get; set; } = new DepositDataSection();
    }

    public class DepositDataSection
    {
        public List<DepositProductItem> DepositProduct { get; set; } = new List<DepositProductItem>();
    }

    public class DepositProductItem
    {
        public string ProductCode { get; set; } = string.Empty;
        public string CampaignId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DetailInfoLink { get; set; } = string.Empty;
        public string InformationMessage { get; set; } = string.Empty;

 
        public List<string> CurrencyCode { get; set; } = new();


        public int? MinTerm { get; set; }
        public int? MaxTerm { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

}

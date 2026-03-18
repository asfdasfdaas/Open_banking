using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    // 1. The Request Object (What we send to the bank)
    public class DepositCalculatorRequest
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public long DepositType { get; set; } // e.g. 55500003
        public long CampaignId { get; set; }  // e.g. 6000002324
        public int TermDays { get; set; }
    }

    // 2. The Response Objects (What the bank sends back)
    public class DepositCalculatorResponse
    {
        public DepositCalcDataSection Data { get; set; } = new DepositCalcDataSection();
    }

    public class DepositCalcDataSection
    {
        public DepositCalcDetails Deposit { get; set; } = new DepositCalcDetails();
    }

    public class DepositCalcDetails
    {
        public decimal? WithholdingRate { get; set; }
        public decimal? WithholdingAmount { get; set; }
        public int? TermDays { get; set; }
        public decimal? NetInterestAmount { get; set; }
        public decimal? NetInterestAmountCurrency { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? WithholdingAmountTL { get; set; }
        public decimal? InterestRate { get; set; }
        public string InformationMessage { get; set; } = string.Empty;
    }
}
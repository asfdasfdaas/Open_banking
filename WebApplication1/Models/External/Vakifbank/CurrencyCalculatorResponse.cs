namespace WebApplication1.Models.External.Vakifbank
{
    public class CurrencyCalculatorResponse
    {
        public CurrencyDataSection Data { get; set; } = new CurrencyDataSection();
    }
    public class CurrencyDataSection
    {
        public CurrencyDetails Currency { get; set; } = new CurrencyDetails();
    }
    public class CurrencyDetails
    {
        public string SaleRate { get; set; } = string.Empty;
        public string TargetCurrencyCode { get; set; } = string.Empty;
        public string SaleAmount { get; set; } = string.Empty;
    }
}

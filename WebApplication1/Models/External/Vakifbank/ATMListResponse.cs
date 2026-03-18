using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class ATMListResponse
    {
        public ATMDataSection Data { get; set; } =new ATMDataSection();
    }

    public class ATMDataSection
    {
        public List<ATMItem> ATM { get; set; } = new();
    }

    public class ATMItem
    {
        public string CityCode { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? LocationCode { get; set; }

        public string Latitude { get; set; } = string.Empty;
        public string ATMName { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? ATMCurrencyCode { get; set; }

        public string Longitude { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? CommonATM { get; set; }

        public string ATMCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? ServiceCode { get; set; }

        public string Neighbourhood { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? IsPhysicallyImpaired { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? IsVisuallyImpaired { get; set; }

        public string ATMAddress { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
    }
}
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class BranchListResponse
    {
        public BranchDataSection Data { get; set; }
    }

    public class BranchDataSection
    {
        public List<BranchItem> Branch { get; set; } = new();
    }

    public class BranchItem
    {
        public string CityCode { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? JewelryDeliveryBranch { get; set; }
        public string BranchAddress { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string PerformanceBranchTypeName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? VisuallyDisabledFriendly { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? SafeDepositBoxStatus { get; set; }
        public string BranchStatus { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? PhysicallyDisabledFriendly { get; set; }


        public int? PerformanceBranchTypeId { get; set; }
        public string DistrictCode { get; set; } = string.Empty;
    }
}

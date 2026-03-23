using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class DistrictListResponse
    {
        public DistrictDataSection Data { get; set; }
    }

    public class DistrictDataSection
    {
        public List<DistrictItem> District { get; set; } = new();
    }

    public class DistrictItem
    {
        public string DistrictName { get; set; } = string.Empty;
        public string NVIDistrictCode { get; set; } = string.Empty;
        public string BankDistrictCode { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
    }
}
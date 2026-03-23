using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class CityListResponse
    {
        public CityDataSection Data { get; set; } = new();
    }

    public class CityDataSection
    {
        public List<CityItem> City { get; set; } = new();
    }

    public class CityItem
    {
        public string CityCode { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
    }
}
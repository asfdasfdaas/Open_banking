using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class NeighborhoodListResponse
    {
        public NeighborhoodDataSection Data { get; set; }
    }

    public class NeighborhoodDataSection
    {
        public List<NeighborhoodItem> Neighborhood { get; set; } = new();
    }

    public class NeighborhoodItem
    {
        public string NeighborhoodName { get; set; } = string.Empty;
        public string NeighborhoodCode { get; set; } = string.Empty;
    }
}
namespace WebApplication1.Models.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetTotal { get; set; }
        
        public List<ChartDataPointDto> ChartData { get; set; } = new();
    }

    public class ChartDataPointDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}

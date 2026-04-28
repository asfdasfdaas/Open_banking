namespace WebApplication1.Models.DTOs
{
    public class DailyLimitDto
    {
        public decimal Limit { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
    }
}

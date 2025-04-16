namespace CommodityTradingAPI.Services
{
    public interface ILog
    {
        string EntityName { get; set; }
        string Action { get; set; }
        string ChangedBy { get; set; }
        DateTime Timestamp { get; set; }
        string Details { get; set; }
    }
}

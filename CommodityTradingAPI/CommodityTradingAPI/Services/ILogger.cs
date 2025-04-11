namespace CommodityTradingAPI.Services
{
    public interface ILogger
    {
        Task LogChangeAsync(ILog auditLog);
    }
}

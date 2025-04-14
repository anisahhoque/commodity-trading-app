namespace CommodityTradingAPI.Services
{
    public interface ILogger
    {
        Task LogChangeAsync(ILog auditLog);

        Task CreateNewLogAsync(string entityName, string action, string changedBy, string details, bool sus);

    }
}

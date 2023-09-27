namespace FutbinSales.Core.Players.Services;

public interface ISalesService
{
    DateTime ConvertToDateTime(string date);

    Task<bool> ScrapeDataAsync(int id);
}
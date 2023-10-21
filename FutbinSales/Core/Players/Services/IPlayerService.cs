namespace FutbinSales.Core.Players.Services;

public interface IPlayerService
{
    public Task<bool> GetPlayers(string URL, Category category);
}
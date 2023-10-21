using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FutbinSales.Core.Players;

public class Player
{
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public Category? Category { get; set; }
    public int? CategoryId { get; set; }
    
    public Player(int id, string? name)
    {
        Id = id;
        Name = name;
    }

    public Player()
    {
    }
    
    
    public IEnumerable<Sale> GetRecentSales()
    {
        int delta = 8;
        // var lastSale = Sales.OrderByDescending(s => s.SaleDate).FirstOrDefault();
        var firstSale = Sales.OrderBy(s => s.SaleDate).FirstOrDefault();
        if (firstSale != null)
        {
            var diff = DateTime.Now - firstSale.SaleDate;
            if (diff.Days >= 3)
            {
                delta = 36;
            }
            else if (diff.Days >= 1 && diff.Days < 3)
            {
                delta = 14;
            }
        }
        var sales = Sales.Where(s => s.SaleDate > DateTime.Now.AddHours(-delta));
        var trimmedSales = sales.OrderBy(s => s.Price).ToList();
        var trimAmount = (int) Math.Round(trimmedSales.Count * 0.02);
        trimmedSales.RemoveRange(0, trimAmount);
        trimmedSales.RemoveRange(trimmedSales.Count - trimAmount, trimAmount);
        return trimmedSales;
    }
    
    public double minPrice => GetRecentSales().Count() > 0 ? GetRecentSales().Min(s => s.Price)/1000 : 0;
    public double maxPrice => GetRecentSales().Count() > 0 ? GetRecentSales().Max(s => s.Price)/1000 : 0;

    [DisplayName("Average Price")]
    public double GetAvgPrice() 
    {
        var avg = Math.Round(GetRecentSales().Count() > 0 ? GetRecentSales().Average(s => s.Price) : 0);
        if (avg > 10000)
        {
            return Math.Round(avg / 1000, 1);
        }
        return avg;
    }
    public double avgPrice => GetAvgPrice();
    public double trimmedLowPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.05)).Min(s => s.Price)/1000 : 0;
    public double trimmedMaxPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.05)).Max(s => s.Price)/1000 : 0;

    [DisplayName ("# Sales")]public int salesCount => GetRecentSales().Count();
    // Find the date of the first sale and calculate how many hours ago that was
    public double dataFromLastXHours => Sales.Count > 0 ? Math.Round((DateTime.Now - Sales.OrderBy(s => s.SaleDate).FirstOrDefault().SaleDate).TotalHours) : 0;

}

public class Sale
{
    public Guid Id { get; set; }
    public int PlayerId { get; set; }
    public int Price { get; set; }
    public DateTime SaleDate { get; set; }
    public Player Player { get; set; } = null!;
    
    public Sale(int playerId, int price, DateTime saleDate)
    {
        Id = Guid.NewGuid();
        PlayerId = playerId;
        Price = price;
        SaleDate = saleDate;
    }
}

public class Category
{
    public int Id { get; set; }
    public string? Name { get; set; }
    private readonly List<Player> _players= new();
    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();
    
    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }
    
    public Category(string name)
    {
        Name = name;
    }
    
    public Category()
    {
    }
}
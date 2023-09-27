using System.ComponentModel;

namespace FutbinSales.Core.Players;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    
    public Player(int id, string name)
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
        var lastSale = Sales.OrderByDescending(s => s.SaleDate).FirstOrDefault();
        if (lastSale != null)
        {
            var diff = DateTime.Now - lastSale.SaleDate;
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
    [DisplayName ("Average Price")]
    public double avgPrice => Math.Round(GetRecentSales().Count() > 0 ? GetRecentSales().Average(s => s.Price)/1000 : 0);
    public double trimmedLowPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.08)).Min(s => s.Price)/1000 : 0;
    public double trimmedMaxPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.08)).Max(s => s.Price)/1000 : 0;

    [DisplayName ("# Sales")]public int salesCount => GetRecentSales().Count();
    // Calculate the trend
 

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
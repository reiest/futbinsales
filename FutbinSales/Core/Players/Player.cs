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
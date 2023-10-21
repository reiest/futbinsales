using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
    
    /// <summary>
    /// Gets the most recent sales for this player,
    /// trims the top and bottom 2% of sales and returns the rest
    /// </summary>
    /// <returns>Recent sales</returns>
    public IEnumerable<Sale> GetRecentSales()
    {
        int delta = 8;
        const int longDelta = 36;
        const int shortDelta = 14;
        const double trimPercentage = 0.02;

        var firstSale = Sales.OrderBy(s => s.SaleDate).FirstOrDefault();

        if (firstSale != null)
        {
            var diff = DateTime.UtcNow - firstSale.SaleDate;

            if (diff.Days >= 3)
            {
                delta = longDelta;
            }
            else if (diff.Days >= 1)
            {
                delta = shortDelta;
            }
        }

        var sales = Sales
            .Where(s => s.SaleDate > DateTime.UtcNow.AddHours(-delta))
            .OrderBy(s => s.Price)
            .ToList();

        var trimAmount = (int)Math.Round(sales.Count * trimPercentage);

        if (sales.Count > trimAmount * 2)
        {
            sales.RemoveRange(0, trimAmount);
            sales.RemoveRange(sales.Count - trimAmount, trimAmount);
        }

        return sales.OrderBy(s => s.SaleDate).ToList();
    }
    
    public IEnumerable<Sale> recentSales => GetRecentSales();
    
    [DisplayName("Min Price")]
    public double minPrice => recentSales.Count() > 0 ? recentSales.Min(s => s.Price) : 0;
    
    [DisplayName("Max Price")]
    public double maxPrice => recentSales.Count() > 0 ? recentSales.Max(s => s.Price) : 0;
    
    [DisplayName("Average Price")]
    public double avgPrice => recentSales.Count() > 0 ? Math.Round(recentSales.Average(s => s.Price),0) : 0;
    
    public string TrendAnalysis()
    {
        int length = recentSales.Count();
        int halfLength = length / 2;

        if (length < 2)
        {
            // Not enough data to determine a trend.
            return "Not enough data";
        }

        Sale[] part1 = recentSales.Take(halfLength).ToArray();
        Sale[] part2 = recentSales.Skip(halfLength).ToArray();

        double avg1 = part1.Average(s => s.Price);
        double avg2 = part2.Average(s => s.Price);

        double trendPercentage = ((avg2 - avg1) / avg1) * 100;

        return $"{Math.Round(trendPercentage, 1)}%";
    }
    
    [DisplayName("Trend")]
    public string trend => TrendAnalysis();
    
    
    public double trimmedLowPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.05)).Min(s => s.Price)/1000 : 0;
    public double trimmedMaxPrice => GetRecentSales().Count() > 0 ? GetRecentSales().OrderBy(s => s.Price).Skip((int) Math.Round(GetRecentSales().Count() * 0.05)).Max(s => s.Price)/1000 : 0;

    [DisplayName ("# Sales")]public int salesCount => GetRecentSales().Count();
    
    public double HoursSinceFirstSale()
    {
        var firstSale = recentSales.FirstOrDefault();
        if (firstSale != null)
        {
            var diff = DateTime.UtcNow - firstSale.SaleDate;
            return Math.Round(diff.TotalHours, 0);
        }

        return 0;
    }
    
    [DisplayName("Hours since first sale")]
    public double dataFromLastXHours => HoursSinceFirstSale();

    public string MostSalesInterval()
    {
        var avg = avgPrice;
        int count = 0;
        int commons = 0;
        int percennn;
        int step;

        step = (avg < 100000) ? 250 : 1000;
        percennn = (int)((avg * 0.025) / step) * step;

        if (percennn < 1000)
        {
            percennn = 1000;
        }

        if (maxPrice - minPrice < percennn * 2)
        {
            percennn = (int)(percennn / 1.75);
        }

        for (int i = (int)minPrice + percennn; i < (int)maxPrice - percennn; i += step)
        {
            int internalcount = 0;
            for (int j = -percennn; j <= percennn; j += step)
            {
                internalcount += recentSales.Count(p => p.Price == i + j);
            }

            if (internalcount > count)
            {
                count = internalcount;
                commons = i;
            }
        }

        Tuple<int, int> interval = new Tuple<int, int>((int)(commons - percennn) / 1000, (int)(commons + percennn) / 1000);
        return $"Interval: ({interval.Item1},{interval.Item2}), Count: {count}";
    }
    
    [DisplayName("Most sales interval")]
    public string mostSalesInterval => MostSalesInterval();
    
    public string CountOccurrencesGreaterThan(int threshold)
    {
        if (recentSales == null)
        {
            throw new ArgumentNullException("numbers");
        }

        int count = recentSales.Count(n => n.Price > threshold);
        double percentage = Math.Round((double)count / recentSales.Count() * 100.0, 1);
        return $"{percentage}%";
    }
    
    [DisplayName("Sales over avg")]
    public string salesOverAvg => CountOccurrencesGreaterThan((int)avgPrice);

    public string SplitInThreeAndCalculateMedians()
    {
        int length = recentSales.Count();
        int partSize = length / 3;
        int pt1 = (int)CalculateMedian(recentSales.Skip(0).Take(partSize).ToList());
        int pt2 = (int)CalculateMedian(recentSales.Skip(partSize).Take(partSize*2).ToList());
        int pt3 = (int)CalculateMedian(recentSales.Skip(partSize*2).Take(partSize*3-1).ToList());

        return $"({pt1}, {pt2}, {pt3})";
    }
    
    private static double CalculateMedian(List<Sale> numbers)
    {
        numbers = numbers.OrderBy(s => s.Price).ToList();
        // make a list of only the prices
        int count = numbers.Count;
        if (count == 0)
        {
            return 0.0; // Return 0 if the list is empty.
        }

        if (count % 2 == 0)
        {
            // If the count is even, calculate the mean of the two middle values.
            int middle = count / 2;
            return (numbers[middle - 1].Price + numbers[middle].Price) / 2.0;
        }
        // If the count is odd, return the middle value.
        return numbers[count / 2].Price;
    }
    
    [DisplayName("Median per third")]
    public string medianPerThird => SplitInThreeAndCalculateMedians();
    
    public double buyPrice => Math.Round(avgPrice * 0.90 / 1000);
    

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
    [Required]
    [DisplayName("Category Name")]
    public string? Name { get; set; }
    private readonly List<Player> _players= new();
    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();
    
    public Category(string name)
    {
        Name = name;
    }
    
    public Category()
    {
    }
}
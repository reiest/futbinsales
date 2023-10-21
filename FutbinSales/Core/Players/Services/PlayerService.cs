using FutbinSales.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FutbinSales.Core.Players.Services;

public class PlayerService : IPlayerService
{
    private readonly HttpClient _httpClient;
    private readonly SalesContext _context;

    public PlayerService(HttpClient httpClient, SalesContext context)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
    }

    public async Task<bool> GetPlayers(string URL, Category category)
    {
        if(URL == null)
        {
            return false;
        }
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(await _httpClient.GetStringAsync(URL));
        var names = doc.DocumentNode.SelectNodes("//td/div/div/a");
        
        if (URL.Contains("/23/"))
        {
            URL = URL.Replace("m/pl", "m/23/pl");
        }
        var pages = doc.DocumentNode.SelectNodes("//ul[@class='pagination']/li/a");
        var numberPages = pages == null ? 1 : int.Parse(pages[pages.Count - 2].InnerText.Trim());

        var links = new List<string>();
        
        foreach (var name in names)
        {
            var link = name.GetAttributeValue("href", "");
            link = "https://www.futbin.com" + link;
            links.Add(link);
        }
        
        if (numberPages >= 2)
        {
            for (int i = 2; i <= numberPages; i++)
            {
                var newLink = URL.Replace("page=1", $"page={i}");
                doc.LoadHtml(await _httpClient.GetStringAsync(newLink));
                names = doc.DocumentNode.SelectNodes("//td/div/div/a");
                foreach (var name in names)
                {
                    var link = name.GetAttributeValue("href", "");
                    link = "https://www.futbin.com" + link;
                    links.Add(link);
                }
            }
        }
        
        string RemoveAccents(string playername)
        {
            var normalized = playername.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
        
        var playerCounter = 0;
        var players = _context.Players.ToList();
        foreach (var link in links)
        {
            doc.LoadHtml(await _httpClient.GetStringAsync(link));
            await Task.Delay(30); // Delay to avoid getting blocked by futbin
            var player_id = doc.DocumentNode.SelectSingleNode("//div[@id='page-info']").GetAttributeValue("data-player-resource", "");
            var cName = doc.DocumentNode.SelectSingleNode("//div[@class='pcdisplay-name']");
            var cardName = cName.InnerText.Trim();
            cardName = RemoveAccents(cardName);
            var rt = doc.DocumentNode.SelectSingleNode("//div[@class='pcdisplay-rat']");
            var rating = rt.InnerText.Trim();

            // Create a new Player entry with the data, or use a database context
            // Example:
            if (!players.Any(p => p.Id == int.Parse(player_id)))
            {
                var player = new Player
                {
                    Id = int.Parse(player_id),
                    Name = cardName + " " + rating,
                    Category = category
                };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }


            playerCounter += 1;
        }

        return true;
        // return Ok("Scraping complete");
    }
}
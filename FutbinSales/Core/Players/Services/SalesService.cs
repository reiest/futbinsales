using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FutbinSales.Data;
using Newtonsoft.Json;

namespace FutbinSales.Core.Players.Services
{
    public class SalesService : ISalesService
    {
        public DateTime ConvertToDateTime(string date)
        {
            DateTime parsedDate;
            string formattedDate = date.Replace("23,", "2023").Replace(" am", "").Replace(" pm", "");
            if (DateTime.TryParseExact(formattedDate, "MMM dd yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.AddHours(2);
            }
            return DateTime.Now;
        }

        private readonly HttpClient _httpClient;
        private readonly SalesContext _context;

        public SalesService(HttpClient httpClient, SalesContext context)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        }

        public async Task<bool> ScrapeDataAsync(int id)
        {
            string apiUrl = $"https://www.futbin.com/getPlayerChart?type=live-sales&resourceId={id}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject(jsonContent);

                    // You can return the parsed result or perform additional processing here.
                    var player = _context.Players.Find(id);
                    List<Sale> sales = new List<Sale>();
                    _context.Sales.RemoveRange(_context.Sales.Where(s => s.PlayerId == id));
                    foreach (var sale in (dynamic)result)
                    {
                        Sale saleEntry = new Sale(id, (int)sale[1], ConvertToDateTime(sale[0].ToString()));
                        saleEntry.Player = player;
                        sales.Add(saleEntry);
                        _context.Sales.Add(saleEntry);
                    }

                    await _context.SaveChangesAsync();
                    
                    // Assuming the operation was successful, return true
                    return true;
                }
                else
                {
                    // Handle HTTP errors (e.g., Forbidden, Not Found, etc.) here.
                    throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network-related errors here.
                throw new HttpRequestException($"HTTP request error: {ex.Message}");
            }
        }
    }
}

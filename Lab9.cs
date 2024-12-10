using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
    {
    private static readonly HttpClient client = new HttpClient();
    private const string ApiToken = "Mmxmc3gxbUM5YWIxUUw5aHZsYnVqdHZMRzJJRUVvLWwxWXJnc2FkT3o0dz0";

    static async Task Main(string[] args)
        {
        if (!File.Exists("ticker.txt"))
            {
            Console.WriteLine("Файл ticker.txt не найден.");
            return;
            }

        var tickers = await File.ReadAllLinesAsync("ticker.txt");
        var startDate = "2020-01-01";
        var endDate = "2022-01-01";

        var averagePrices = new Dictionary<string, double>();

        var tasks = new List<Task>();

        foreach (var ticker in tickers)
            {
            tasks.Add(GetAveragePriceAndStore(ticker, startDate, endDate, averagePrices));
            }

        await Task.WhenAll(tasks);

        foreach (var kvp in averagePrices)
            {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

    private static async Task GetAveragePriceAndStore(string ticker, string startDate, string endDate, Dictionary<string, double> averagePrices)
        {
        try
            {
            var averagePrice = await GetAveragePrice(ticker, startDate, endDate);
            if (averagePrice.HasValue)
                {
                averagePrices[ticker] = averagePrice.Value;
                }
            }
        catch (Exception ex)
            {
            Console.WriteLine($"Ошибка при обработке {ticker}: {ex.Message}");
            }
        }

    private static async Task<double?> GetAveragePrice(string ticker, string startDate, string endDate)
        {
        var url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={startDate}&to={endDate}&token={ApiToken}";

        try
            {
            var response = await client.GetStringAsync(url);
            var data = JObject.Parse(response);

            if (data["s"].ToString() != "ok")
                {
                Console.WriteLine($"Ошибка при получении данных для {ticker}: {data["msg"]}");
                return null;
                }

            double totalPrice = 0;
            int count = 0;

            foreach (var entry in data["data"])
                {
                double high = (double)entry["h"];
                double low = (double)entry["l"];
                totalPrice += (high + low) / 2;
                count++;
                }

            return count > 0 ? totalPrice / count : (double?)null;
            }
        catch (HttpRequestException httpEx)
            {
            Console.WriteLine($"Ошибка HTTP для {ticker}: {httpEx.Message}");
            return null;
            }
        catch (Exception ex)
            {
            Console.WriteLine($"Ошибка при обработке {ticker}: {ex.Message}");
            return null;
            }
        }
    }
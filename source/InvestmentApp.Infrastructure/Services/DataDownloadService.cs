using InvestmentApp.Application.Services;
using InvestmentApp.Domain.Entities;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;

namespace InvestmentApp.Infrastructure.Services;

public class DataDownloadService(HttpClient httpClient) : IDataDownloadService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<(HttpStatusCode, List<StockData>)> GetStock(Ticker ticker)
    {
        // Get tomorrow at midnight
        DateTime startDate = DateTime.Today.AddDays(1);

        // Convert to Unix timestamp (seconds since January 1, 1970 UTC)
        long startDateUnixTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();

        // Get tomorrow at midnight
        DateTime endDate = startDate.AddYears(-2);

        // Convert to Unix timestamp (seconds since January 1, 1970 UTC)
        long endDateUnixTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

        string query = $"v8/finance/chart/{ticker.TickerSymbol}?period1={endDateUnixTimestamp}&period2={startDateUnixTimestamp}&interval=1d&includePrePost=true&events=div%7Csplit%7Cearn&lang=en-US&region=US&source=cosaic";
        string url = $"{_httpClient.BaseAddress}{query}";
        HttpRequestMessage msg = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };
        //msg.Headers.Add(":authority", "query2.finance.yahoo.com");
        //msg.Headers.Add(":method", "GET");
        //msg.Headers.Add(":path", query);
        //msg.Headers.Add(":scheme", "https");
        msg.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        msg.Headers.Add("accept-encoding", "gzip, deflate, br, zstd");
        msg.Headers.Add("accept-language", "en-US,en;q=0.9");
        msg.Headers.Add("cookie", "GUC=AQEBCAFpYNxpjEIeWgSi&s=AQAAAN4Zvbzu&g=aV-V_Q; A1=d=AQABBCuyRWkCEPa5kl1pIf0_vUH9cW5jjqMFEgEBCAHcYGmMadxK0iMA_eMDAAcIK7JFaW5jjqM&S=AQAAAlKlmTd86GAtgGmuG-htq30; A3=d=AQABBCuyRWkCEPa5kl1pIf0_vUH9cW5jjqMFEgEBCAHcYGmMadxK0iMA_eMDAAcIK7JFaW5jjqM&S=AQAAAlKlmTd86GAtgGmuG-htq30; _cb=BKsRKTBybEZrBNT_5k; A1S=d=AQABBCuyRWkCEPa5kl1pIf0_vUH9cW5jjqMFEgEBCAHcYGmMadxK0iMA_eMDAAcIK7JFaW5jjqM&S=AQAAAlKlmTd86GAtgGmuG-htq30; gpp=DBAA; gpp_sid=-1; fes-ds-Polymarket-Spotlight=1; fes-ds-spotlight=1; PRF=t%3DPH; fes-ds-session=pv%3D5; _chartbeat2=.1767871988410.1770283759969.0000000000000001.CJez6UByEe1RCbdZfKCNt-gbDOJ7N1.4; cmp=t=1770334187&j=0&u=1---");
        msg.Headers.Add("priority", "u=0, i");
        msg.Headers.Add("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
        msg.Headers.Add("sec-ch-ua-mobile", "?0");
        msg.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        msg.Headers.Add("sec-fetch-dest", "document");
        msg.Headers.Add("sec-fetch-mode", "navigate");
        msg.Headers.Add("sec-fetch-site", "none");
        msg.Headers.Add("sec-fetch-user", "?1");
        msg.Headers.Add("upgrade-insecure-requests", "1");
        msg.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36");
        try
        {

            var response = await _httpClient.SendAsync(msg);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return (response.StatusCode, []);
            }

            // Read the compressed content as a stream
            Stream compressedStream = await response.Content.ReadAsStreamAsync();

            // Decompress based on the compression type
            Stream decompressedStream;

            // Check the Content-Encoding header to determine compression type
            string? contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

            if (contentEncoding == "gzip")
            {
                decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            }
            else if (contentEncoding == "deflate")
            {
                decompressedStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            }
            else if (contentEncoding == "br")
            {
                decompressedStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
            }
            else
            {
                // Not compressed or unknown compression
                decompressedStream = compressedStream;
            }

            // Read the decompressed data
            using StreamReader reader = new(decompressedStream);
            string dataReaderJson = await reader.ReadToEndAsync();
            JObject jsonObject = JObject.Parse(dataReaderJson);
            var timestampDataSet = GetListFromJsonString<long>(jsonObject, "$.chart.result[0].timestamp");
            var openDataSet = GetListFromJsonString<decimal>(jsonObject, "$.chart.result[0].indicators.quote[0].open");
            var highDataSet = GetListFromJsonString<decimal>(jsonObject, "$.chart.result[0].indicators.quote[0].high");
            var lowDataSet = GetListFromJsonString<decimal>(jsonObject, "$.chart.result[0].indicators.quote[0].low");
            var closeDataSet = GetListFromJsonString<decimal>(jsonObject, "$.chart.result[0].indicators.quote[0].close");
            var volumeDataSet = GetListFromJsonString<decimal>(jsonObject, "$.chart.result[0].indicators.quote[0].volume");
            var dataSet = ReaderResultToStockList(timestampDataSet,
                openDataSet, closeDataSet, highDataSet, lowDataSet,
                volumeDataSet, ticker);
            return (response.StatusCode, dataSet);
        }
        catch
        {
            throw;
        }
    }

    private static List<T> GetListFromJsonString<T>(JObject jsonObject, string jsonPath)
    {
        List<T> dataSet = [];

        JArray dataSetArray = (JArray)jsonObject.SelectToken(jsonPath)!;

        if (dataSetArray is not null)
        {
            foreach (JToken dataPoint in dataSetArray)
            {
                try
                {
                    if (dataPoint.Type != JTokenType.Null)
                    {
                        T dataPointObject = dataPoint.ToObject<T>()!;
                        dataSet.Add(dataPointObject);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.StackTrace);
                }
            }
        }

        return dataSet;
    }

    private static List<StockData> ReaderResultToStockList(List<long> timestampDataSet,
        List<decimal> openDataSet, List<decimal> closeDataSet, List<decimal> highDataSet,
        List<decimal> lowDataSet, List<decimal> volumeDataSet,
        Ticker ticker)
    {
        int minCount = Math.Min(timestampDataSet.Count, Math.Min(openDataSet.Count, Math.Min(closeDataSet.Count,
            Math.Min(highDataSet.Count, Math.Min(lowDataSet.Count, volumeDataSet.Count)))));
        List<StockData> StockList = [];
        for (int i = 0; i < minCount; i++)
        {
            var stock = new StockData(ticker.TickerSymbol,
                                                    openDataSet[i],
                                                    highDataSet[i],
                                                    lowDataSet[i],
                                                    closeDataSet[i],
                                                    (long)volumeDataSet[i],
                                                    DateTimeOffset.FromUnixTimeSeconds(timestampDataSet[i]).DateTime);
            StockList.Add(stock);
        }
        return StockList;
    }
}

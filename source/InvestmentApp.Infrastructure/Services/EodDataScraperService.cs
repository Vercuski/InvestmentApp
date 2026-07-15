using InvestmentApp.Application.Abstractions;
using InvestmentApp.Domain.Entities;
using InvestmentApp.Domain.Options;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Net;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using SeleniumCookie = OpenQA.Selenium.Cookie;

namespace InvestmentApp.Infrastructure.Services;

/// <summary>
/// Downloads ticker symbol lists from eoddata.com. A headless Chrome instance
/// (via Selenium) logs into the member area once to establish an authenticated
/// session; the resulting session cookies are then reused on a plain HttpClient
/// to pull each exchange's symbol list without re-driving the browser per request.
/// </summary>
public class EodDataScraperService(IOptions<EodDataOptions> eodDataOptions) : IEodDataScraperService
{
    private const string LoginUrl = "https://eoddata.com/Login.aspx";
    private const string SymbolListUrlTemplate = "https://eoddata.com/Data/symbollist.aspx?e={0}";
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

    public async Task<List<Ticker>> DownloadSymbolsAsync(IEnumerable<ExchangePoint> exchangeCodes, CancellationToken cancellationToken)
    {
        var exchanges = exchangeCodes.Where(e => !string.IsNullOrWhiteSpace(e.ExchangeSymbol)).ToList();
        if (exchanges.Count == 0)
        {
            return [];
        }

        CookieContainer cookieContainer = LoginAndGetSessionCookies();

        using var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        using var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        var tickers = new List<Ticker>();
        var seenSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var exchangeCode in exchanges)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string url = string.Format(SymbolListUrlTemplate, Uri.EscapeDataString(exchangeCode.ExchangeSymbol!));
            string csvContent;
            try
            {
                csvContent = await httpClient.GetStringAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download symbol list for exchange '{exchangeCode}': {ex.Message}");
                continue;
            }

            foreach (var ticker in ParseSymbolListCsv(csvContent))
            {
                if (seenSymbols.Add(ticker.TickerSymbol!))
                {
                    ticker.ExchangeSymbol = exchangeCode.ExchangeSymbol;
                    tickers.Add(ticker);
                }
            }
        }

        return tickers;
    }

    /// <summary>
    /// Uses a headless Chrome instance to log into eoddata.com's member login form,
    /// then returns the resulting session cookies for reuse on a plain HttpClient.
    /// The browser is only used for the login step; it is closed immediately after.
    /// </summary>
    private CookieContainer LoginAndGetSessionCookies()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());

        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless=new");
        chromeOptions.AddArgument("--no-sandbox");
        chromeOptions.AddArgument("--disable-dev-shm-usage");
        chromeOptions.AddArgument("--disable-gpu");
        chromeOptions.AddArgument("--window-size=1920,1080");
        chromeOptions.AddArgument($"--user-agent={UserAgent}");

        using var driver = new ChromeDriver(chromeOptions);
        try
        {
            driver.Navigate().GoToUrl(LoginUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            // eoddata's login control renders its inputs with IDs like
            // ctl00_cph1_Login1_UserName (ASP.NET WebForms flattens the naming
            // container into the rendered id). Matching on "Login1" rather than a
            // hard-coded full id is more resilient to markup changes.
            var usernameField = wait.Until(d => d.FindElement(
                By.CssSelector("input[id*='Login1'][type='text'], input[id*='Login1'][type='email']")));
            var passwordField = driver.FindElement(By.CssSelector("input[id*='Login1'][type='password']"));

            usernameField.Clear();
            usernameField.SendKeys(eodDataOptions.Value.Username);
            passwordField.Clear();
            passwordField.SendKeys(eodDataOptions.Value.Password + Keys.Enter);


            // The Login control renders as a LinkButton (an <a> tag that triggers
            // __doPostBack), not a submit <input>, so it needs to be clicked directly.
            //var loginLink = driver.FindElement(By.CssSelector("a[id*='btnLogin']"));
            //loginLink.Click();

            // Wait for the member login form to disappear, which indicates the
            // postback completed.
            wait.Until(d => !d.PageSource.Contains("MEMBER LOGIN", StringComparison.OrdinalIgnoreCase));

            var cookieContainer = new CookieContainer();
            foreach (SeleniumCookie cookie in driver.Manage().Cookies.AllCookies)
            {
                cookieContainer.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path ?? "/", cookie.Domain));
            }

            return cookieContainer;
        }
        finally
        {
            driver.Quit();
        }
    }

    /// <summary>
    /// Parses the CSV returned by eoddata.com's symbol list download. Assumes a
    /// "Code,Name" style layout with an optional header row. Inspect a live
    /// response once you have credentials wired up and adjust the column
    /// handling here if the actual format differs.
    /// </summary>
    private static IEnumerable<Ticker> ParseSymbolListCsv(string csvContent)
    {
        using var reader = new StringReader(csvContent);
        string? line;
        bool isFirstLine = true;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = line.Split('\t');

            if (isFirstLine)
            {
                isFirstLine = false;
                string firstField = fields[0].Trim().Trim('"');
                if (firstField.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                    firstField.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            string symbol = fields[0].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(symbol))
            {
                continue;
            }

            string? description = fields.Length > 1 ? fields[1].Trim().Trim('"') : null;

            yield return new Ticker
            {
                TickerSymbol = symbol,
                Description = description
            };
        }
    }
}

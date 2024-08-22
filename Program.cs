using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace HttpClientInoa
{
    public class Quotation
    {
        public string? Ticker { get; set; }
        public float? CurrentPrice { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided.");
            }
            else
            {
                string ticker = args[0];
                float sellingPrice = float.Parse(args[1]);
                float buyingPrice = float.Parse(args[2]);

                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine($"Argument {i + 1}: {args[i]}");
                }
                RunAsync(ticker, sellingPrice, buyingPrice).GetAwaiter().GetResult();
            }
        }

        static async Task RunAsync(string ticker, float sellingPrice, float buyingPrice)
        {
            string apiUrl = "https://cotacao.b3.com.br/mds/api/v1/instrumentQuotation/" + ticker;

            // HTTP Client initialization
            using HttpClient client = new HttpClient();

            try
            {
                // GET request to API
                var response = await client.GetFromJsonAsync<QuotationResponse>(apiUrl);

                if (response!= null && response.Trad != null && response.Trad.Length > 0 && response.Trad[0].Scty != null && response.Trad[0].Scty.SctyQtn != null && response.Trad[0].Scty.SctyQtn.CurPrc != null)
                {
                    var currentPrice = response.Trad[0].Scty.SctyQtn.CurPrc;
                    Console.WriteLine($"Current Price: {currentPrice}");
                }
                else
                {
                    Console.WriteLine("Invalid response from the API.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }



            try
            {
                Quotation quotation = new Quotation()
                {
                    Ticker = ticker,
                    CurrentPrice = 0,
                };

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    
    public class QuotationResponse
    {
        public Trad[]? Trad { get; set; }
    }

    public class Trad
    {
        public Scty? Scty { get; set; }

    }

    public class Scty
    {
        public SctyQtn? SctyQtn { get; set; }
    }

    public class SctyQtn
    {
        public double? CurPrc { get; set; }

    }
}


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;
using Microsoft.Extensions.Configuration;
using System.Timers;

using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HttpClientInoa
{
    class Program
    {
        static void Main(string[] args)
        {
        // Linking file to appsettings.json
            string fileName = "appsettings.json";
            string fullPath = Path.GetFullPath(fileName);
            fullPath = fullPath.Remove(fullPath.LastIndexOfAny(new char[] { '\\' }, fullPath.LastIndexOf('\\') ));

            var config = new ConfigurationBuilder()
                .SetBasePath(fullPath)
                .AddJsonFile(fileName, optional: false, reloadOnChange: true)
                .Build();

            // Get configuration data 
            var toEmail = config["AppSettings:ToEmail"];
            var toName = config["AppSettings:ToName"];
            var apiUrl = config["AppSettings:ApiUrl"];
            int getCurrentPriceMinutesInterval = Int32.Parse(config["AppSettings:GetCurrentPriceMinutesInterval"]);
            string emailApiKey = config["AppSettings:EmailApiKey"];
            

            Console.WriteLine($"Email: {toEmail}");
            Console.WriteLine($"API URL: {apiUrl}");

        
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided.");
            }
            else
            {
                string ticker = args[0];
                float buyingPrice = float.Parse(args[1]);
                float sellingPrice = float.Parse(args[2]);
                string transactionType = "";

                Console.WriteLine($"ticker: {args[0]}");
                Console.WriteLine($"buyingPrice: {args[1]}");
                Console.WriteLine($"sellingPrice: {args[2]}");

                while (1==1){ //run until you close the aplication
                    float currentPrice = GetCurrentPrice(ticker, apiUrl).GetAwaiter().GetResult();
                    if (currentPrice >= sellingPrice || currentPrice <= buyingPrice){
                        transactionType = currentPrice <= buyingPrice ? "buy" : "sell";
                        SendEmail(ticker, currentPrice, transactionType, emailApiKey, toEmail, toName);
                    }
                    var secondsWaited = WaitGetInterval(getCurrentPriceMinutesInterval).GetAwaiter().GetResult();   
                    Console.WriteLine($"secondsWaited: {secondsWaited}");
                }
            }
        }
        static async Task<int> WaitGetInterval(int minutes){
            int loops = minutes*60/3;
            for(int i = 0; i < loops; i++){
                Thread.Sleep(3000);
            }
            return loops*3;
        }

        static async Task<float> GetCurrentPrice(string ticker, string apiUrl)
        {
            apiUrl += ticker;

            // HTTP Client initialization
            using HttpClient client = new HttpClient();

            try
            {
                // GET request to Financial API
                var response = await client.GetFromJsonAsync<QuotationResponse>(apiUrl);

                if (response!= null && response.Trad != null && response.Trad.Length > 0 && response.Trad[0].Scty != null && response.Trad[0].Scty.SctyQtn != null && response.Trad[0].Scty.SctyQtn.CurPrc != null)
                {
                    float currentPrice = response.Trad[0].Scty.SctyQtn.CurPrc;
                    Console.WriteLine($"Current Price: {currentPrice}");
                    return currentPrice;
                }
                else
                {
                    throw new Exception("Invalid response from the API.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception("Could not get Current Price from API");
            }

        }

        static void SendEmail(string ticker, float currentPrice, string transactionType, string emailApiKey, string toEmail, string toName)
        {
            if(!Configuration.Default.ApiKey.ContainsKey("api-key"))
                Configuration.Default.ApiKey.Add("api-key", emailApiKey);

            var apiInstance = new TransactionalEmailsApi();
            string SenderName = "Vagner Morais";
            string SenderEmail = "vagnermoraiss@hotmail.com";
            SendSmtpEmailSender Email = new SendSmtpEmailSender(SenderName, SenderEmail);
            SendSmtpEmailTo smtpEmailTo = new SendSmtpEmailTo(toEmail, toName);
            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            To.Add(smtpEmailTo);
            List<SendSmtpEmailBcc> Bcc = new List<SendSmtpEmailBcc>();
            string CcName = "John Doe";
            string CcEmail = "vagnermoraiss@hotmail.com";
            SendSmtpEmailCc CcData = new SendSmtpEmailCc(CcEmail, CcName);
            List<SendSmtpEmailCc> Cc = new List<SendSmtpEmailCc>();
            Cc.Add(CcData);
            string HtmlContent = "<html><body><h1>Hey, buddy! Time to {{params.transactionType}} {{params.ticker}} on B3!</h1></body></html>";
            string Subject = "{{params.ticker}} {{params.subject}}";
            string stringInBase64 = "aGVsbG8gdGhpcyBpcyB0ZXN0";
            byte[] Content = System.Convert.FromBase64String(stringInBase64);
            JObject Headers = new JObject();
            Headers.Add("Some-Custom-Name", "unique-id-1234");
            long? TemplateId = null;
            JObject Params = new JObject();
            Params.Add("ticker", ticker);
            Params.Add("transactionType", transactionType);
            Params.Add("subject", "Quotation News");
            List<string> Tags = new List<string>();
            Tags.Add("mytag");
            SendSmtpEmailTo1 smtpEmailTo1 = new SendSmtpEmailTo1(toEmail, toName);
            List<SendSmtpEmailTo1> To1 = new List<SendSmtpEmailTo1>();
            To1.Add(smtpEmailTo1);
            Dictionary<string, object> _params = new Dictionary<string, object>();
            _params.Add("params", Params);
            SendSmtpEmailMessageVersions messageVersion = new SendSmtpEmailMessageVersions(to: To1, _params: _params,subject: Subject);
            List<SendSmtpEmailMessageVersions> messageVersiopns = new List<SendSmtpEmailMessageVersions>();
            messageVersiopns.Add(messageVersion);
            try
            {
                var sendSmtpEmail = new SendSmtpEmail(sender: Email, to: To, htmlContent: HtmlContent, subject: Subject, headers: Headers, templateId: TemplateId, _params: Params, messageVersions: messageVersiopns, tags: Tags);
                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                Debug.WriteLine(result.ToJson());
                Console.WriteLine("Email sent!");

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Console.WriteLine("Email not sent!");

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
        public float CurPrc { get; set; }

    }
}


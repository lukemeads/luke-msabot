using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application1.Controllers
{
    public class RatesBot
    {
        public static async Task<double?> GetExchangeRateAsync(string RateSymbol)
        {
            HttpClient client = new HttpClient();
            Rates.RootObject rootObject;
            string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest"));
            rootObject = JsonConvert.DeserializeObject<Rates.RootObject>(x);

            double rate = rootObject.rates.getRate(RateSymbol);
            return rate;
        }
    }
}
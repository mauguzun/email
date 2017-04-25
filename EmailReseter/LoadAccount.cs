using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReseter
{
    public class LoadAccount
    {
         public string Path { get; set; }

        public List<Account> GetAccountList(string @blasters,string @base)
        {
           
            Dictionary<string, Account> acc = new Dictionary<string, Account>();

            string[] blaster = File.ReadAllLines(@blasters);
            foreach (string line in blaster)
            {
                string[] blParam = line.Split(':');
                
                if (!acc.Keys.Contains(blParam[0]))
                {
                    acc.Add(blParam[0],new Account() { Email = blParam[0], PinPassword = blParam[1], Nick = blParam[2] });
                }
                else
                {
                    File.AppendAllText(this.Path +"/" + "!miseedEmail.txt", line + Environment.NewLine);
                }

            }
            string[] _base = File.ReadAllLines(@base);
            foreach (string line in _base)
            {
                string[] blParam = line.Split(':');
                if (acc.Keys.Contains(blParam[0]))
                    acc[blParam[0]].EmailPassword = blParam[1];
            }
            return acc.Values.ToList();


        }

    }
}

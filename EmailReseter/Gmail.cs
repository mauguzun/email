using EAGetMail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReseter
{
    [Serializable]
    class Gmail
    {
        public string ClearEmail { get; set; }
        public string RealEmail { get; set; }
        public string Header  { get; set; }
        public string  Body { get; set; }

        public string To { get; set; }
        public DateTime Date { get; set; }

        public Mail Eagtmail { get; set; }

        public override string ToString()
        {
            return $"{ClearEmail},{RealEmail},{Environment.NewLine}";
        }

    }
}

using EAGetMail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReseter
{
    public  class ClientEmail
    {
        public MailClient Cline { get; set; }
        public string  Email { get; set; }
        public bool Connection { get; set; } = true;
    }
}

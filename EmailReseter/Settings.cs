using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReseter
{
    public class Settings
    {
        public string GetEmailPassword(string email)
        {
            if (email.Contains("@brauksimkopa.lv") || email.Contains("@antikvar.lv"))
                return "De171717!";
            if (email.Contains("@gmail.com"))
                return Form1.gmailpassword;
            else
                return null;
        }
        public string GetEmailHost(string host)
        {


            if (host.Contains("@mail.ru"))
                return "pop.mail.ru";
            else if (host.Contains("@gmail.com"))
                return "imap.gmail.com";
            else if (host.Contains("@bk.ru"))
                return "pop.bk.ru";
            else if (host.Contains("@inbox.ru"))
                return "pop.inbox.ru";
            else if (host.Contains("@mail.ua"))
                return "pop.mail.ua";
            else if (host.Contains("internet-veikals.lv"))
                return "mail.internet-veikals.lv";
            else if (host.Contains("brauksimkopa.lv"))
                return "brauksimkopa.lv";
            else if (host.Contains("antikvar.lv"))
                return "antikvar.lv";
            else
                return null;

        }
    }
}

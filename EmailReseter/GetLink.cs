using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EmailReseter
{
    public class GetLink
    {
        public string FindLink(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a");
            foreach (HtmlNode link in links)
                if (link.InnerText.ToLower().Contains("this was me") || link.InnerText.ToLower().Contains("try logging in again")
                    ||  link.InnerText.ToLower().Contains("reset") ||  link.InnerText.ToLower().Contains("confirm your email")    || link.InnerText.ToLower().Contains("сбросить"))
                {
                    return link.GetAttributeValue("href", null);
                }

            //Confirm your email
            return null;
        }
       

    }
}

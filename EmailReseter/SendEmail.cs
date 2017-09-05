using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Mail;

namespace EmailReseter
{
    public class SendEmail
    {

        public void  Send (string body)
        {
            var fromAddress = new MailAddress("mauguzun@gmail.com", "Deniss");
            var toAddress = new MailAddress("mauguzun@gmail.com", "Deniss");
             string fromPassword = Form1.gmailpassword;
            
            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            try
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "back up",
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch(Exception ex)
            {

            }
           

        }
      
    }
}

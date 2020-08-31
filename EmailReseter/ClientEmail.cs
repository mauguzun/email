using EAGetMail;

namespace EmailReseter
{
    public  class ClientEmail
    {
        public MailClient Cline { get; set; }
        public string  Email { get; set; }
        public bool Connection { get; set; } = true;
    }
}

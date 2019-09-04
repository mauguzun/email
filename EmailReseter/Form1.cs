using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using System.IO;
using EAGetMail;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using HtmlAgilityPack;
using System.Diagnostics;

namespace EmailReseter
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private string Path { get; set; }
        List<Account> acc;

        private string _blaster = "source_all_account_for_blaster.txt";
        private string _base = "source_all_account.txt";
        private string _num = "___num.txt";
        private string _res;

        static public int count = 0;

        static public string gmailpassword;

        static List<Gmail> _gmails;


        public Form1()
        {
            InitializeComponent();
            Test();
            _Init();

       
            

        }


        private void Test()
        {
            try
            {
                //ChromeOptions options = new ChromeOptions();
                //ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                //service.SuppressInitialDiagnosticInformation = true;
                //service.HideCommandPromptWindow = true;
                //options.AddArgument("--log-level=3");
                //options.AddArgument("headless");
                //RemoteWebDriver driver = new ChromeDriver(service, options);

                //driver.Quit();
            }
            catch
            {
                MessageBox.Show("cant make driver");
            }

        }
        private void _Init()
        {
            this.Path = pathTxt.Text.Trim();
            _gmails = new List<Gmail>();


            if (!Directory.Exists(Path + System.IO.Path.DirectorySeparatorChar + "res"))
                Directory.CreateDirectory(Path + System.IO.Path.DirectorySeparatorChar + "res");
            this._res = Path + System.IO.Path.DirectorySeparatorChar + "res";

            if (!File.Exists(this._res + System.IO.Path.DirectorySeparatorChar + _num))
                File.WriteAllText(this._res + System.IO.Path.DirectorySeparatorChar + _num, "1");

        }

        private void clickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (acc == null)
                return;

            myCons.Text = $"gmail done {Environment.NewLine} start read email ,boss";


            Parallel.For(0, acc.Count, new ParallelOptions { MaxDegreeOfParallelism = 20 }, Read);
            #region
            //for (int i = 0; i < acc.Count; i++)
            //{
            //    myCons.Text = acc[i].Email + Environment.NewLine +  myCons.Text;
            //    Account temp = Read(acc[i]);
            //    if (temp.PinPassword != acc[i].PinPassword)
            //    {
            //        acc[i] = temp;
            //        dataGridView.Update();
            //        dataGridView.Refresh();
            //    }

            //}
            #endregion;
            this.Update();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (acc != null)
                this.Update();



            myCons.Text = "load data" + Environment.NewLine;
            var loadAcc = new LoadAccount() { Path = this.Path };
            acc = loadAcc.GetAccountList(Path + '/' + _blaster, Path + '/' + _base, Path);

            // we put all 
            var allmail = acc.Where(ex => ex.Email.Contains(".gmail.com")).ToList();


            if (acc == null)
            {
                MessageBox.Show("dont find gmail  password");
            }

            dataGridView.DataSource = acc;
            dataGridView.Columns[2].Visible = false;
            dataGridView.Columns[3].Visible = false;

            this.Text = acc.Count.ToString() + "_accounts ";
            foreach (DataGridViewColumn column in dataGridView.Columns)
                dataGridView.Columns[column.Name].SortMode = DataGridViewColumnSortMode.Automatic;
        }

        private void setPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(pathTxt.Text.Trim()))
                myCons.Text = Environment.NewLine + "Directory not exist pls check path";
            else
            {
                this.Path = pathTxt.Text.Trim();
                this._Init();
            }

        }
        //private static PhantomJSDriverService _GetJsSettings()
        //{
        //    var serviceJs = PhantomJSDriverService.CreateDefaultService();
        //    serviceJs.HideCommandPromptWindow = true;
        //    return serviceJs;
        //}
        public void Read(int x)
        {

            count++;
            // this.Text = count.ToString();

            Account account = acc[x];
            string pass = (new Settings().GetEmailPassword(account.Email) == null)
                ? account.EmailPassword : new Settings().GetEmailPassword(account.Email);


            var serverproto = (account.Email.Contains("@gmail.com")) ? ServerProtocol.Imap4 : ServerProtocol.Pop3;


            MailServer oServer = new MailServer(new Settings().GetEmailHost(account.Email), account.Email, pass, serverproto);
            MailClient oClient = new MailClient("TryIt");
           // oClient.LogFileName = "maillog.txt";
            // Please add the following codes:
            oServer.SSLConnection = true;
            oServer.Port = (account.Email.Contains("@gmail.com")) ? 993 : 995;
            account.Time = DateTime.Now;
            try
            {
                oClient.Connect(oServer);
                MailInfo[] infos = oClient.GetMailInfos();
                infos = infos.Reverse().ToArray();

                /////////////// here :) just read all 

                int gmailCount = account.Email.Contains("gmail") ? 125 : 25;


                for (int i = 0; i < gmailCount; i++)
                {

                    string resetEmailUrl = null;

                    MailInfo info = infos[i];
                    Mail oMail;


                    oMail = oClient.GetMail(info);


                    if (oMail.HtmlBody.Contains("suspended your Pinterest"))
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(oMail.HtmlBody);
                        HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a");
                        foreach (HtmlNode link in links)
                        {

                            if (link.InnerText.Contains("this link"))
                            {

                                var driver = GetDriver();
                                driver.Navigate().GoToUrl(link.GetAttributeValue("href", null));
                                Thread.Sleep(5000);
                                driver.Quit();
                                myCons.Text = "spam policy";
                                oClient.Delete(info);
                            }

                        }
                        continue;
                    }
                    else if (oMail.HtmlBody.Contains("Confirm your email"))
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(oMail.HtmlBody);
                        HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a");
                        foreach (HtmlNode link in links)
                        {

                            if (link.InnerText.Contains("Confirm your email"))
                            {

                                var driver = GetDriver();

                                driver.Navigate().GoToUrl(link.GetAttributeValue("href", null));
                                Thread.Sleep(5000);
                                driver.Quit();
                                myCons.Text = "Confirm";
                                oClient.Delete(info);
                            }

                        }
                        continue;
                    }



                    if ((DateTime.Today - oMail.SentDate).TotalHours > 12)
                        continue;

                    var subj = oMail.Subject;
                    var rep = oMail.To[0].Address;

                    //if (account.Email.Contains("@gmail.com"))
                    //{
                    //    if (account.Email != rep)
                    //    {
                    //        continue;
                    //    }
                    //}

                    //false
                    if (oMail.From.Address.ToString().Contains("pinterest"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);


                    if (resetEmailUrl != null)
                    {
                        TryReset(account, resetEmailUrl, rep);
                       
                    }

                 

                }

                // Quit and pure emails marked as deleted from POP3 server.

                acc[x] = account;
                dataGridView.Update();
                dataGridView.Refresh();

            }
            catch (Exception ep)
            {

                try
                {
                    File.AppendAllText(this._res + System.IO.Path.DirectorySeparatorChar + "total_bad.txt", account.Email + ep.Message + Environment.NewLine);
                    account.Status = ep.Message;
                }
                catch { }


            }
            finally
            {
                try
                {
                    oClient.Quit();
                }
                catch { }
            }



        }

        public Account Read(Account acc)
        {

            string pass = (new Settings().GetEmailPassword(acc.Email) == null) ? acc.EmailPassword : new Settings().GetEmailPassword(acc.Email);

            MailServer oServer = new MailServer(new Settings().GetEmailHost(acc.Email), acc.Email, acc.EmailPassword, ServerProtocol.Pop3);
            MailClient oClient = new MailClient("TryIt");
            // Please add the following codes:
            oServer.SSLConnection = true;
            oServer.Port = 995;

            if (acc.Email.Contains("gmail"))
            {
                oServer.Port = 993;
            }


            acc.Time = DateTime.Now;
            try
            {
                oClient.Connect(oServer);
                MailInfo[] infos = oClient.GetMailInfos();
                infos = infos.Reverse().ToArray();
                for (int i = 0; i < 25; i++)
                {

                    string resetEmailUrl = null;
                    MailInfo info = infos[i];
                    Mail oMail = oClient.GetMail(info);
                    if ((DateTime.Today - oMail.SentDate).TotalHours > 12)
                        continue;

                    var subj = oMail.Subject;
                    var rep = oMail.To[0].Address;

                    if (acc.Email.Contains("@gmail.com"))
                    {
                        if (acc.Email != rep)
                        {
                            continue;
                        }
                    }

                    if (acc.Email == "dinm.o.b.ilweb.app.re.l@gmail.com")
                    {
                        var x = 0;
                    }

                    //false
                    if (oMail.From.Address.ToString().Contains("pinterest.com"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);


                    if (resetEmailUrl != null)
                    {
                        TryReset(acc, resetEmailUrl, rep);
                        // yea we do reset here 
                    }
                    File.AppendAllText(this._res + '/' + acc.Nick + ".html",
                        "<h1>" + acc.Email + "</h1><h3>" + oMail.From.Address + "</h3><h3>" + oMail.ReceivedDate + "</h3><br>" + oMail.HtmlBody + "<br><br><br>");

                    // Mark email as deleted from POP3 server.
                    oClient.Delete(info);
                }

                // Quit and pure emails marked as deleted from POP3 server.
                oClient.Quit();
                return acc;
            }
            catch (Exception ep)
            {
                File.AppendAllText(this._res + System.IO.Path.DirectorySeparatorChar + "total_bad.txt", acc.Email + Environment.NewLine);
                acc.Status = ep.Message;
                return acc;
            }



        }


        public string GetPassword()
        {
            string text = File.ReadAllText(this._res + '/' + this._num);
            int num = 1;
            Int32.TryParse(text, out num);
            num++;
            File.WriteAllText(this._res + '/' + this._num, num.ToString());
            return num.ToString() + "trance";
        }
        private string SetPassWordMinusOne()
        {
            string text = File.ReadAllText(this._res + '/' + this._num);
            int num = 1;
            Int32.TryParse(text, out num);
            num--;
            File.WriteAllText(this._res + '/' + this._num, num.ToString());
            return num.ToString() + "trance";
        }


        private void Update()
        {


            if (acc == null || acc.Count() == 0)
                return;




            try
            {
                File.WriteAllText(this.Path + '/' + this._blaster, "");
                foreach (Account ac in acc)
                    File.AppendAllText(this.Path + '/' + this._blaster, ac.Email + ":" + ac.PinPassword + ":" + ac.Nick + Environment.NewLine);
            }
            catch
            {
                File.WriteAllText(this.Path + '/' + "temp_" + this._blaster, "");
                foreach (Account ac in acc)
                    File.AppendAllText(this.Path + '/' + this._blaster, ac.Email + ":" + ac.PinPassword + ":" + ac.Nick + Environment.NewLine);


            }

            string body = File.ReadAllText(this.Path + '/' + this._blaster);
            body += Environment.NewLine;

            body += File.ReadAllText(this.Path + '/' + this._base);

            SendEmail send = new SendEmail();
            send.Send(body);


            Uploader.MakePost(this.acc);
            // udpate mongo 



            myCons.Text = "Done ";
        }


        private void myCons_TextChanged(object sender, EventArgs e)
        {
            if (myCons.Lines.Count() > 20)
                myCons.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Update();
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int i = e.RowIndex;
                myCons.Text = acc[i].Email + Environment.NewLine + myCons.Text;
                Account temp = Read(acc[i]);
                if (temp.PinPassword != acc[i].PinPassword)
                {
                    acc[i] = temp;
                    dataGridView.Update();
                    dataGridView.Refresh();

                }
                this.Update();
            }
            catch (Exception ex)
            {
                myCons.Text = ex.Message;
            }

        }

        private void updateGoodToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string[] good = File.ReadAllLines(this.Path + @"/res/good.txt");
            List<Account> goodAcc = new List<Account>();
            foreach (string line in good)
            {
                string[] lineArr = line.Split(':');
                goodAcc.Add(new Account() { Email = lineArr[0], PinPassword = lineArr[1] });
            }

            acc = new LoadAccount().GetAccountList(Path + '/' + _blaster, Path + '/' + _base, Path);
            if (acc == null)
            {
                MessageBox.Show("dont find gmail  password");
            }


            for (int i = 0; i < acc.Count(); i++)
            {
                var query = from g in goodAcc
                            where g.Email == acc[i].Email
                            select g.PinPassword;

                if (query.Count() > 0)
                {
                    acc[i].PinPassword = query.LastOrDefault();
                }
            }

            var x = acc;


            this.Update();
        }

        private void tryFindLostedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //load acc

            myCons.Text = "load data" + Environment.NewLine;
            var loadAcc = new LoadAccount() { Path = this.Path };
            acc = loadAcc.GetAccountList(Path + '/' + _blaster, Path + '/' + _base, Path);

            var allGmail = acc.FindAll(w => w.Email.Contains("@gmail.com"));

            List<string> done = new List<string>();

            foreach (var acc in allGmail)
            {

                myCons.Text = acc.Email;
                string clearEmail = acc.Email.Replace(".", "");
                if (done.Contains(clearEmail))
                {
                    continue;
                }
                done.Add(clearEmail);

                string pass = (new Settings().GetEmailPassword(acc.Email) == null) ? acc.EmailPassword : new Settings().GetEmailPassword(acc.Email);

                MailServer oServer = new MailServer(new Settings().GetEmailHost(acc.Email), acc.Email, pass, ServerProtocol.Pop3);
                MailClient oClient = new MailClient("TryIt");
                // Please add the following codes:
                oServer.SSLConnection = true;
                oServer.Port = 995;
                acc.Time = DateTime.Now;
                try
                {
                    oClient.Connect(oServer);
                    MailInfo[] infos = oClient.GetMailInfos();
                    infos = infos.Reverse().ToArray();
                    for (int i = 0; i < 75; i++)
                    {

                        string resetEmailUrl = null;
                        MailInfo info = infos[i];
                        Mail oMail = oClient.GetMail(info);
                        if ((DateTime.Today - oMail.SentDate).TotalHours > 24)
                            continue;

                        var subj = oMail.Subject;
                        var rep = oMail.To[0].Address;
                        myCons.Text += i.ToString();

                        var x = allGmail.FindAll(em => em.Email == rep);

                        if (allGmail.FindAll(em => em.Email == rep).Count > 0)
                            continue;

                        //false
                        if (oMail.From.Address.ToString().Contains("pinterest"))
                            resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);


                        if (resetEmailUrl != null)
                        {
                            TryReset(acc, resetEmailUrl, rep, "missed.txt");

                            // yea we do reset here 
                        }
                        File.AppendAllText(this._res + '/' + acc.Nick + ".html",
                            "<h1>" + acc.Email + "</h1><h3>" + oMail.From.Address + "</h3><h3>" + oMail.ReceivedDate + "</h3><br>" + oMail.HtmlBody + "<br><br><br>");

                        // Mark email as deleted from POP3 server.
                        oClient.Delete(info);
                    }

                    // Quit and pure emails marked as deleted from POP3 server.
                    oClient.Quit();

                }
                catch (Exception ep)
                {
                    File.AppendAllText(this._res + System.IO.Path.DirectorySeparatorChar + "total_bad.txt", acc.Email + Environment.NewLine);

                }
            }
            //naiti vse gmail
            // proverit vse pochti gmail gde netu acc
        }

        private RemoteWebDriver GetDriver()
        {
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            options.AddArgument("--log-level=3");
          options.AddArgument("headless");
            RemoteWebDriver driver = new ChromeDriver(service, options);
            return driver;

        }



        private void TryReset(Account acc, string resetEmailUrl, string rep, string fileName = "good.txt")
        {
            var driver = GetDriver();
            try
            {
                driver.Navigate().GoToUrl(resetEmailUrl);
                //Thread.Sleep(5000);
                string tempPassword = GetPassword();
                driver.FindElementById("newPassword").SendKeys(tempPassword);
                driver.FindElementById("confirmNewPassword").SendKeys(tempPassword);
                driver.FindElementByXPath("//button[@type='submit']").Click();
                acc.PinPassword = tempPassword;
                Thread.Sleep(7000);
               // Uploader.MakePost(this.acc);
                File.AppendAllText(this._res + '/' + fileName, rep + ':' + acc.PinPassword + Environment.NewLine);
                if (acc.Email.Contains("@gmail.com"))
                {
                    if (acc.Email != rep)
                    {
                        this.acc.FirstOrDefault(x => x.Email == rep).PinPassword = tempPassword;
                    }
                }
            }
            catch (Exception ex)
            {
                SetPassWordMinusOne();
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}

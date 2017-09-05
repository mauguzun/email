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
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace EmailReseter
{
    public partial class Form1 :System.Windows.Forms.Form
    {
        private string Path { get; set; }
        List<Account> acc;

        private string _blaster = "source_all_account_for_blaster.txt";
        private string _base = "source_all_account.txt";
        private string _num = "___num.txt";
        private string _res;

        static public string gmailpassword;


        public Form1()
        {
            InitializeComponent();
            _Init();
           
        }
        
        private void _Init()
        {
            this.Path = pathTxt.Text.Trim();

           
            if (!Directory.Exists(Path + System.IO.Path.DirectorySeparatorChar + "res"))
                Directory.CreateDirectory(Path + System.IO.Path.DirectorySeparatorChar + "res");
            this._res = Path + System.IO.Path.DirectorySeparatorChar + "res";

            if (!File.Exists(this._res + System.IO.Path.DirectorySeparatorChar + _num))
                File.WriteAllText(this._res + System.IO.Path.DirectorySeparatorChar + _num,"1");

        }

        private void clickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (acc == null)
                return;

            myCons.Text = "gmail " + Form1.gmailpassword;

            myCons.Text += "start read email ,boss :)";

            Parallel.For(0, acc.Count, new ParallelOptions { MaxDegreeOfParallelism = 5 },    Read );

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
            this.Update();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (acc != null)
                this.Update();

            myCons.Text = "load data" + Environment.NewLine;
            var loadAcc = new LoadAccount() { Path = this.Path };
            acc = loadAcc.GetAccountList(Path + '/' + _blaster,Path + '/' + _base,Path);
            if (acc == null)
            {
                MessageBox.Show("dont find gmail  password");
            }

            dataGridView.DataSource = acc;
            dataGridView.Columns[2].Visible = false;
            dataGridView.Columns[3].Visible = false;

            this.Text = acc.Count.ToString() + "_accounts " ;
            foreach (DataGridViewColumn column in dataGridView.Columns)
                dataGridView.Columns[column.Name].SortMode = DataGridViewColumnSortMode.Automatic;
        }

        private void setPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(pathTxt.Text.Trim()))
                myCons.Text = Environment.NewLine + "Directory not exist pls check path"  ;
            else
            {
                this.Path = pathTxt.Text.Trim();
                this._Init();
            }
                
        }
        private static PhantomJSDriverService _GetJsSettings()
        {
            var serviceJs = PhantomJSDriverService.CreateDefaultService();
            serviceJs.HideCommandPromptWindow = true;
            return serviceJs;
        }
        public void Read(int x)
        {

            Account account = acc[x];
            string pass = (new Settings().GetEmailPassword(account.Email) == null) ? account.EmailPassword : new Settings().GetEmailPassword(account.Email);


            var serverproto = (account.Email.Contains("@gmail.com")) ? ServerProtocol.Imap4 : ServerProtocol.Pop3;
           

            MailServer oServer = new MailServer(new Settings().GetEmailHost(account.Email), account.Email, pass, serverproto);
            MailClient oClient = new MailClient("TryIt");
            // Please add the following codes:
            oServer.SSLConnection = true;
            oServer.Port = (account.Email.Contains("@gmail.com")) ? 993: 995;
            account.Time = DateTime.Now;
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

                    if (account.Email.Contains("@gmail.com"))
                    {
                        if (account.Email != rep)
                        {
                            continue;
                        }
                    }

                    //false
                    if (oMail.From.Address.ToString().Contains("ohno"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    else if (oMail.Subject.Contains("reset"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    else if (oMail.From.Address.Contains("suspicious") || oMail.From.Address.Contains("Get back on Pinterest") || oMail.From.Address.Contains("confirm") || oMail.From.Address.Contains("reactivate"))
                    {
                        var sibe = oMail.Subject;
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    }

                    if (resetEmailUrl != null)
                    {
                        RemoteWebDriver driver;

                        if (toolStripComboBoxDriver.Selected.ToString() == "Chrome")
                            driver = new ChromeDriver();
                        else
                            driver = new PhantomJSDriver(_GetJsSettings());

                        try
                        {
                           

                            driver.Navigate().GoToUrl(resetEmailUrl);
                            Thread.Sleep(5000);
                            string tempPassword = GetPassword();
                            driver.FindElementById("newPassword").SendKeys(tempPassword);
                            driver.FindElementById("confirmNewPassword").SendKeys(tempPassword);
                            driver.FindElementByXPath("//button[@type='submit']").Click();
                            account.PinPassword = tempPassword;
                            Thread.Sleep(7000);

                            File.AppendAllText(this._res + '/' + "good.txt", account.Email + ':' + account.PinPassword + Environment.NewLine);
                            driver.Quit();
                        }
                        catch (Exception ex)
                        {
                            SetPassWordMinusOne();

                        }
                        finally
                        {
                           
                            driver.Quit();
                        }

                        // yea we do reset here 
                    }
                    File.AppendAllText(this._res + '/' + account.Nick + ".html",
                        "<h1>" + account.Email + "</h1><h3>" + oMail.From.Address + "</h3><h3>" + oMail.ReceivedDate + "</h3><br>" + oMail.HtmlBody + "<br><br><br>");

                    // Mark email as deleted from POP3 server.
                    oClient.Delete(info);
                }

                // Quit and pure emails marked as deleted from POP3 server.
                oClient.Quit();
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



        }

        public Account Read( Account acc)
        {

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

                    //false
                    if (oMail.From.Address.ToString().Contains("ohno"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    else if (oMail.Subject.Contains("reset"))
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    else if (oMail.Subject.Contains("suspicious") || oMail.Subject.Contains("confirm") || oMail.Subject.Contains("reactivate"))
                    {
                        resetEmailUrl = new GetLink().FindLink(oMail.HtmlBody);
                    }

                    if (resetEmailUrl != null)
                    {
                        var driver = new ChromeDriver();
                        driver.Manage().Window.Size = new Size(-2000, 0);
                        try
                        {
                            driver.Navigate().GoToUrl(resetEmailUrl);
                            Thread.Sleep(5000);
                            string tempPassword = GetPassword();
                            driver.FindElementById("newPassword").SendKeys(tempPassword);
                            driver.FindElementById("confirmNewPassword").SendKeys(tempPassword);
                            driver.FindElementByXPath("//button[@type='submit']").Click();
                            acc.PinPassword = tempPassword;
                            Thread.Sleep(7000);
                            
                            File.AppendAllText(this._res + '/' + "good.txt", acc.Email + ':' + acc.PinPassword + Environment.NewLine);
                        }
                        catch(Exception ex)
                        {
                            SetPassWordMinusOne();
                        }
                        finally
                        {
                            driver.Quit();
                        }
                        
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
            return num.ToString()+"trance";
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
            


            myCons.Text = "Done ";
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Update();
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
                int i = e.RowIndex ;
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
           
            string [] good = File.ReadAllLines(this.Path + @"/res/good.txt");
            List<Account> goodAcc = new List<Account>();
            foreach(string line in good)
            {
                string[] lineArr = line.Split(':');
                goodAcc.Add(new Account() { Email = lineArr[0], PinPassword = lineArr[1] });
            }

            acc = new LoadAccount().GetAccountList(Path + '/' + _blaster, Path + '/' + _base ,Path);
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
                    acc[i].PinPassword = query.FirstOrDefault();
                }
            }


            
           
            this.Update();
        }
    }
}

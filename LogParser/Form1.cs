using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;

namespace LogParser
{
    public partial class Form1 : Form
    {
        private String[] _files = { "googlem.txt", "google.txt", "msn.txt", "bing.txt" };
        private DateTime _start_time = new DateTime();

        public Form1()
        {
            InitializeComponent();
        }

        private void ProcessLine(String line, String filename)
        {
            Match m = Regex.Match(line, "([0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3})", RegexOptions.None);
            if (m.Groups.Count > 0)
            {
                using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + filename, true))
                {
                    sw.WriteLine(m.Groups[0].Value);
                    sw.Close();
                }
            }
        }

        private void ProcessUrl(String url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 30 * 60 * 1000;
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = request.Credentials; 
            WebResponse response = (WebResponse)request.GetResponse();
            using (Stream s = response.GetResponseStream())
            {
                using (StreamReader log = new StreamReader(s))
                {
                    for (; ; )
                    {
                        if (log.EndOfStream)
                            break;

                        String line = log.ReadLine();
                        if (line.Contains("Googlebot-Mobile"))
                            ProcessLine(line, _files[0]);
                        else if (line.Contains("Googlebot"))
                            ProcessLine(line, _files[1]);
                        else if (line.Contains("msnbot"))
                            ProcessLine(line, _files[2]);
                        else if (line.Contains("bingbot"))
                            ProcessLine(line, _files[3]);
                    }
                    log.Close();
                }
            }
        }

        private void RemoveDups(String file)
        {
            List<String> IPs = new List<String>();  
            using (StringReader reader = new StringReader(File.ReadAllText(file)))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    if (!IPs.Contains(line))
                        IPs.Add(line);
            }
            using (StreamWriter writer = new StreamWriter(File.Open(file, FileMode.Create)))
                foreach (string value in IPs)
                    writer.WriteLine(value); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            button1.Enabled = false;
            timer1.Enabled = true;
            _start_time = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           // foreach (String file in _files)
          //      File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + file);

            String[] urls = File.ReadAllLines(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\input.txt");
            foreach (String url in urls)
                ProcessUrl(url);

            foreach (String file in _files)
                RemoveDups(file);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            button1.Enabled = true;
            timer1.Enabled = false;
            MessageBox.Show("Ok");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - _start_time;
            label2.Text = ts.TotalSeconds.ToString("0.00");
        }

    }
}

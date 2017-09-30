using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WUApiLib;

namespace LinearCompair
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            initsupportedKB();
            initLB();
        }

        private void initsupportedKB()
        {
            _supportedKB = new List<string>();
            
        }

        private List<string> _supportedKB;
        private void initLB()
        {
            string line;

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader("MasterRecord.txt");
            while ((line = file.ReadLine()) != null)
            {
                dataGridView1.Rows.Add(line);
                _supportedKB.Add(line);
            }

            file.Close();
        }


        private async void checkButton_ClickAsync(object sender, EventArgs e)
        {
            
            await Task.Factory.StartNew(() => searcherAsync());
            
        }

        
        private async Task searcherAsync()
        {
            UpdateSession uSession = new UpdateSession();
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
            ISearchResult uResult = uSearcher.Search("IsInstalled=0 and Type=Security");

            UpdateCollection updateCollection = new UpdateCollection();
            string _subString;
            foreach(IUpdate update in uResult.Updates)
            {
                _subString = ExtractString(update.Title);
                foreach (string kb in _supportedKB)
                {
                    if (kb == _subString)
                    {
                        gridUpdate(kb, 2, "Found");
                    }
                }
            }
            foreach (IUpdate update in uResult.Updates)
            {

                _subString = ExtractString(update.Title);
                
                foreach(string kb in _supportedKB)
                {
                    if (kb == _subString)
                    {
                        updateCollection.Add(update);
                        gridUpdate(kb, 2, "Downloaded");
                        await Task.Run(() => download(uSession, updateCollection));

                        gridUpdate(kb, 2, "Downloaded");

                        gridUpdate(kb, 2, "Installing");
                        install(uSession, updateCollection);
                        updateCollection.RemoveAt(0);
                    }
                }
            }
        }
        private void install(UpdateSession uSession,UpdateCollection updateCollection)
        {
            IUpdateInstaller installer = uSession.CreateUpdateInstaller();
            installer.Updates = updateCollection;

            IInstallationResult installationRes = installer.Install();
            string _subString;
            
            for (int i = 0; i < updateCollection.Count; i++)
            {
                
                _subString = ExtractString(updateCollection[i].Title);
                if (installationRes.GetUpdateResult(i).HResult == 0)
                {
                    gridUpdate(_subString, 2, "Installed");
                }
                else
                {
                    gridUpdate(_subString, 2, "Failed");
                }
            }
        }
        private void gridUpdate(string kb,int cell, string text)
        {
            dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Enabled = false));
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                if (kb == dataGridView1.Rows[i].Cells[0].Value.ToString())
                {
                    dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Rows[i].Cells[cell].Value = text));
                }

            }
            dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Enabled = true));
        }
        private async Task previousInstallCheck()
        {
            var updateSession = new UpdateSession();
            var updateSearcher = updateSession.CreateUpdateSearcher();
            var count = updateSearcher.GetTotalHistoryCount();
            var history = updateSearcher.QueryHistory(0, count);

            dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Visible = false));

            foreach (string s in _supportedKB)
            {
                gridUpdate(s, 1, "Missing");
                string splitstring;
                for (int i = 0; i < count; ++i)
                {
                    splitstring = ExtractString(history[i].Title);
                    if(splitstring == s)
                    {
                        gridUpdate(s, 1, "Installed");
                    }
                }
                
            }

            dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Visible = true));
        }
        private void download(UpdateSession uSession,UpdateCollection uCollection)
        {
            UpdateDownloader downloader = uSession.CreateUpdateDownloader();
            downloader.Updates = uCollection;
            downloader.Download();
        }
        string ExtractString(string s)
        {
            // You should check for errors in real-world code, omitted for brevity
            try
            {
                var startTag = "(";
                int startIndex = s.IndexOf(startTag) + startTag.Length;
                int endIndex = s.IndexOf(")", startIndex);
                return s.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return ("CNVFL");
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => previousInstallCheck());
        }
    }
}

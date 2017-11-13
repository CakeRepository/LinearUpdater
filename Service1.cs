using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderService
{
    public partial class Service1 : ServiceBase
    {
        BackgroundQueue bq;
        public Service1()
        {
            InitializeComponent();

        }
        
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Folder Watch Has started.");
            bq = new BackgroundQueue();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Folder Watch Has Stopped.");
        }

        /// <summary>
        /// On file creation file transfer checks if Folder or File and checks if Locked
        /// </summary>
        /// <param name="fullPath">Full path of file being put into CopyFromDir</param>
        /// <param name="name">Name of file from CopyFromDir</param>
        void work(string fullPath, string name)
        {
            string sub = fullPath.Substring(ConfigurationManager.AppSettings["CopyFromDir"].Length);
            string copyItem = ConfigurationManager.AppSettings["CopyToDir"] + sub;

            

            if (name.Contains('.')) 
            {
                Directory.CreateDirectory(Path.GetDirectoryName(copyItem));
                try
                {

                    File.Copy(fullPath, ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                    EventLog.WriteEntry(fullPath + " copied to " + ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);


                }
                catch
                {
                    Thread.Sleep(1500);
                    //s.EventLog.WriteEntry("error copying files");
                    File.Copy(fullPath, ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                    EventLog.WriteEntry(fullPath + " copied to " + ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                }
            }
            else
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["CopyToDir"] + sub);
            }
        }
        

        private void fileSystemWatcher1_Created_1(object sender, FileSystemEventArgs e)
        {
            bq.QueueTask(() => work(e.FullPath, e.Name));
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {

        }
    }
}

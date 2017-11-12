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
using System.Threading.Tasks;

namespace FolderService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Folder Watch Has started.");
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Folder Watch Has Stopped.");
        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            //Not Handled
        }
        

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            
            LockandTransfer LAT = new LockandTransfer();
            FileInfo fileInfo = new FileInfo(e.FullPath);
            BackgroundQueue bgQueue = new BackgroundQueue();

            bgQueue.QueueTask(() => LAT.work(fileInfo,e.FullPath,e.Name));
            
            
        }

        private void fileSystemWatcher1_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            //not handled
        }

        private void fileSystemWatcher1_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            //not handled
        }
    }
}

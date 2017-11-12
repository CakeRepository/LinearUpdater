using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderService
{
    class LockandTransfer
    {        
        public void work(FileInfo fileInfo, string fullPath, string name)
        {
            string sub = fullPath.Substring(ConfigurationManager.AppSettings["CopyFromDir"].Length);
            string[] subFolders = findSubFolders(sub);
            Service1 s = new Service1();
            
            bool isDir = dirCheck(subFolders[subFolders.Length - 1]);
            
            try
            {
                if (isDir == false)
                {
                    //check lock Working but not looped
                    //If working loop add here and run this to check inuse
                    //bool b = CheckLock(fileInfo);

                    File.Copy(fullPath, ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                    s.EventLog.WriteEntry(fullPath + " copied to " + ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                }
                else
                {
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["CopyToDir"] + sub);
                    s.EventLog.WriteEntry(fullPath + " Directory created at " + ConfigurationManager.AppSettings["CopyToDir"] + "\\" + name);
                }

            }
            catch
            {
                s.EventLog.WriteEntry("error copying files");
            }
        }
               
        #region Workers
        string[] findSubFolders(string sub)
        {
            char delimiter = '\\';
            string[] substrings = sub.Split(delimiter);
            return substrings;
        }
        bool dirCheck(string tester)
        {
            bool res = false;

            if (Path.GetExtension(tester) == "")
            {
                //checks if file or DIR
                res = true;
            }
            return res;
        }
        bool CheckLock(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked

            return false;
        }
        #endregion
    }
}

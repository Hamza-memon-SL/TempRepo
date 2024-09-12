using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace secureacceptance.Helper
{
    public class Logging
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void Log(string logMessage)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                string fileName = "Payment_Log_" + dateTime.Year.ToString() + dateTime.Month.ToString() + dateTime.Day.ToString() + ".txt";
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + fileName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path.Replace(fileName, ""));

                // Set Status to Locked
                _readWriteLock.EnterWriteLock();

                try
                {
                    // Append text to the file
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(string.Format("{0}:{1}", dateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), logMessage));
                        sw.Close();
                    }
                }
                finally
                {
                    // Release lock
                    _readWriteLock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
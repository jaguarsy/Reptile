using Reptile.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reptile
{
    public partial class MainForm : IWatcher
    {
        private const string logTemplate = "{0} - at {1}\n";
        private void update(string message)
        {
            if (rtbLog.Lines.Length > 100) rtbLog.Clear();
            rtbLog.AppendText(string.Format(logTemplate, message, DateTime.Now.ToLongTimeString()));
            rtbLog.ScrollToCaret();
        }

        public void Log(string message)
        {
            BeginInvoke(new Action<string>(update), message);
        }
    }
}

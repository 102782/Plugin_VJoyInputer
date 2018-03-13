using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JoyInputer
{
    class SimpleLogger
    {
        private List<string> logs;
        private Object thisLock = new Object();
        private bool enable;

        public enum TAG
        {
            INFO,
            ERROR,
            DEBUG
        }

        public SimpleLogger()
        {
            this.enable = false;
            this.logs = new List<string>();
            this.logs.Add("----------------");
        }

        public void Enabled()
        {
            this.enable = true;
        }

        public void Disabled()
        {
            this.enable = false;
        }

        public void Add(TAG tag, string logText)
        {
            if (this.enable || ((!this.enable) && (tag == TAG.ERROR)))
            {
                lock (thisLock)
                {
                    this.logs.Add(string.Format("[{0}][{1}]{2}", DateTime.Now, tag, logText));
                }
            }
        }

        public void Save(string path)
        {
            if (this.enable)
            {
                using (StreamWriter w = new StreamWriter(path, true, Encoding.UTF8))
                {
                    this.logs.Select(t => { w.WriteLine(t); return t; }).ToArray();
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            this.logs.Select(t => { result += t + Environment.NewLine; return t; }).ToArray();
            return result;
        }
    }
}

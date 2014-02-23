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

        public enum TAG
        {
            INFO,
            ERROR
        }

        public SimpleLogger()
        {
            this.logs = new List<string>();
            this.logs.Add("----------------");
        }

        public void Add(TAG tag, string logText)
        {
            lock (thisLock)
            {
                this.logs.Add(string.Format("[{0}][{1}]{2}", DateTime.Now, tag, logText));
            }
        }

        public void Save(string path)
        {
            using (StreamWriter w = new StreamWriter(path, true, Encoding.UTF8))
            {
                this.logs.Select(t => { w.WriteLine(t); return t; }).ToArray();
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

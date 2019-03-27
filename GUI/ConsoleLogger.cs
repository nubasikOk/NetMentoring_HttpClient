using NetMentoring_HttpClient;
using NetMentoring_HttpClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool isLogEnabled;

        public ConsoleLogger(bool isLogEnabled)
        {
            this.isLogEnabled = isLogEnabled;
        }

        public void Log(string message)
        {
            if (isLogEnabled)
            {
                Console.WriteLine(message);
            }
        }
    }
}

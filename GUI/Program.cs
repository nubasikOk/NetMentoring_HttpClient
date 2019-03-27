using NetMentoring_HttpClient;
using NetMentoring_HttpClient.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo("D:\\site");
            IContentSaver contentSaver = new ContentSaver(rootDirectory);
            ILogger logger = new ConsoleLogger(true);
            ICrawler crawler = new Crawler("http://epam.com", contentSaver, 3, logger);
            try
            {
                crawler.LoadFromUrl("http://epam.com");
            }
            catch (Exception ex)
            {
                logger.Log($"Error during site downloading: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}

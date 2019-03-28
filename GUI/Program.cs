using GUI.DomainConstraint;
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
            Uri uri = new Uri("http://epam.com");
            IDomainConstraint constraint = new CrossDomainTransitionConstraint(CrossDomainTransition.CurrentDomainOnly, uri);
            ICrawler crawler = new Crawler(contentSaver, 3, constraint, logger);
            try
            {
                crawler.LoadFromUrl(uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.Log($"Error during site downloading: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}

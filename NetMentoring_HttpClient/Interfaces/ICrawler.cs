using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMentoring_HttpClient.Interfaces
{
    public interface ICrawler
    {
        int MaxDeepLevel { get; set; }
        void LoadFromUrl(string url);
    }
}

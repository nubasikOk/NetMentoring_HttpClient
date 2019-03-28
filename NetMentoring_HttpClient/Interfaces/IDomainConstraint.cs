using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMentoring_HttpClient.Interfaces
{
    public interface IDomainConstraint
    {
        bool IsAcceptable(Uri uri);
    }
}

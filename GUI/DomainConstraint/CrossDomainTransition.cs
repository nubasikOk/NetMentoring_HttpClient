using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DomainConstraint
{
    public enum CrossDomainTransition
    {
        All = 1,
        CurrentDomainOnly = 2,
        DescendantUrlsOnly = 3
    }
}

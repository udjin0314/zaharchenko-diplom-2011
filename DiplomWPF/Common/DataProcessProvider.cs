using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    interface DataProcessProvider
    {
        Process getProcessValues(Process process);
    }
}

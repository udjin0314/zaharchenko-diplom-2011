using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common;

namespace DiplomWPF.ServerSide
{
    class DataProcessProviderServer : DataProcessProvider 
    {
        public ChislProcess getProcessValues(ChislProcess process)
        {
            return ProcessExecutor.Instance.getProcess(process);
        }
    }
}

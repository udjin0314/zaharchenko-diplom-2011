using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common;
using DiplomWPF.ServerSide;

namespace DiplomWPF.Client
{
    class DataProcessProviderClient : DataProcessProvider
    {
        public ChislProcess getProcessValues(ChislProcess process)
        {
            DataProcessProviderServer processProvider = new DataProcessProviderServer();
            return processProvider.getProcessValues(process);
        }

        
    }
}

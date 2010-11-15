using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common;

namespace DiplomWPF.ServerSide
{
    interface Executor
    {
        Process getProcess(Process processIn);
    }
}

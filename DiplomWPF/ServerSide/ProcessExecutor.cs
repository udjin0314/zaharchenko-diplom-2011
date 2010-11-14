using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common;

namespace DiplomWPF.ServerSide
{
    class ProcessExecutor
    {
        private static ProcessExecutor instance;

        private ProcessExecutor() { }

        public static ProcessExecutor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProcessExecutor();
                }
                return instance;
            }
        }

        public Process process { get; set; }

        public Process getProcess(Process processIn)
        {
            //ProcessChislExecuter procExec = new ProcessChislExecuter(processIn);
            Executor executor = new ChislExecuter();
            return executor.getProcess(processIn);
            /*Double maxT = 0;
            Double minT = Double.MaxValue;
            process = processIn;
            process.values = new Double[process.I, process.J, process.N];
            process.hr = process.R / process.I;
            process.hz = process.L / process.J;
            process.ht = (Double)process.T / process.N;
            for (int n = 0; n < process.N; n++)
                for (int i = 0; i < process.I; i++)
                    for (int j = 0; j < process.J; j++)
                    {
                        Double r = i * process.hr;
                        Double z = j * process.hz;
                        Double t = n * process.ht;
                        Double u = 0;
                        if (r == 0) u = 1;
                        else if (z == 0) u = 1;
                        else if (t == 0) u = 1;
                        else u = Math.Abs( process.P * process.alphaR * process.alphaZ * Math.Sin(r) * Math.Cos(z) * Math.Cos(t) * Math.Sin(t) * process.L * Math.Cos(z * r * t) / r * z * t);
                        if (u > maxT) 
                            maxT = u;
                        if (u < minT)
                            minT = u;
                        process.values[i, j, n] = u;
                    }
            process.maxTemperature = maxT;
            process.minTemperature = minT;
            return process;*/
        }
    }
}

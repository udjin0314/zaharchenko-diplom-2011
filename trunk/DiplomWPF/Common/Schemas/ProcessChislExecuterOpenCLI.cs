using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCLNet;
using DiplomWPF.Common;
using System.IO;

namespace DiplomWPF.ServerSide
{
    class ProcessChislExecuterOpenCLI
    {
        Platform openCLPlatform;
        Device[] openCLDevices;
        Context openCLContext;
        CommandQueue openCLCQ;
        Program procProgProgram;
        Kernel procProgKernel;
        Mem procProgMemBuffer;
        ChislProcess process;

        public ProcessChislExecuterOpenCLI(ChislProcess process)
        {
            openCLPlatform = OpenCL.GetPlatform(0);
            this.process = process;
            openCLDevices = openCLPlatform.QueryDevices(DeviceType.ALL);
            openCLContext = openCLPlatform.CreateDefaultContext();
            openCLCQ = openCLContext.CreateCommandQueue(openCLDevices[0], CommandQueueProperties.PROFILING_ENABLE);
            procProgProgram = openCLContext.CreateProgramWithSource(File.ReadAllText("ServerSide/resolver.cl"));
            try
            {
                procProgProgram.Build();
            }
            catch (OpenCLException)
            {
                string buildLog = procProgProgram.GetBuildLog(openCLDevices[0]);
            }
            procProgKernel = procProgProgram.CreateKernel("resolver");

            //mandelbrotMemBuffer = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.WRITE_ONLY), width*height*4, IntPtr.Zero);
        }




        public ChislProcess getProcess(ChislProcess processIn)
        {
            throw new NotImplementedException();
        }
    }
}

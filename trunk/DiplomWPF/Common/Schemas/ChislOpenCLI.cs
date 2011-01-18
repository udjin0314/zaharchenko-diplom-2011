using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Cloo;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;
using DiplomWPF.Common.Mathem;

namespace DiplomWPF.Common.Schemas
{
    class ChislOpenCLI : ChislProcess
    {
        public static String KERNEL_FILE = "Common/Schemas/OpenCL/PismenRekford.cpp";
        private ComputeContext context;
        private ComputeProgram program;
        private ComputeCommandQueue commands;
        private ICollection<ComputeEventBase> events;
        private ComputeKernel kernelFirst;
        private ComputeKernel kernelSecond;
        private ComputeKernel kernelG;
        private ComputeKernel kernelFr;
        private ComputeKernel kernelFFl;
        private ComputeKernel kernelFl;
        private ComputeKernel kernelBFirst;
        private ComputeKernel kernelFFr;
        private ComputeKernel kernelBSecond;

        private ComputeBuffer<int> IGCL;

        private ComputeBuffer<float> FrACl;
        private ComputeBuffer<float> FrBCl;
        private ComputeBuffer<float> FrCCl;

        private ComputeBuffer<float> FFlACl;
        private ComputeBuffer<float> FFlBCl;
        private ComputeBuffer<float> FFlCCl;

        private ComputeBuffer<float> FCl;
        private ComputeBuffer<float> tempLayerCl;

        private ComputeBuffer<float> paramsRCl;
        private ComputeBuffer<float> paramsLCl;
        private ComputeBuffer<float> paramsGCl;

        private ComputeBuffer<float> GCl;

        private long[] localWorkersJ = null;
        private long[] localWorkersI = null;

        private long[] localWorkersIJ = null;

        private long[] localWorkers3J = null;

        private long[] localWorkers3I = null;

        private float[] FrA;

        private float[] FFlA;

        public ComputePlatform Platform { get; set; }
        public ComputeDevice Device { get; set; }

        public ChislOpenCLI(String name, Brush brush)
            : base(name, brush)
        {
        }

        public void findOptimalLocalWorkers()
        {
            int baseV = 16;
            int locWI = MathHelper.findMaxDiv(I + 1, baseV);
            int locWJ = MathHelper.findMaxDiv(J + 1, baseV);
            int locW3I = MathHelper.findMaxDiv(3 * I + 1, baseV);
            int locW3J = MathHelper.findMaxDiv(3 * J + 1, baseV);
            localWorkersI = new long[] { locWI };
            localWorkersJ = new long[] { locWJ };
            localWorkersIJ = new long[] { locWI, locWJ };
            localWorkers3I = new long[] { locW3I };
            localWorkers3J = new long[] { locW3J };

        }

        public void InitPrograms()
        {
            ComputeContextPropertyList properties = new ComputeContextPropertyList(Platform);
            context = new ComputeContext(new[] {Device} , properties, null, IntPtr.Zero);
            program = new ComputeProgram(context, System.IO.File.ReadAllText(KERNEL_FILE));
            program.Build(new[] { Device }, "", null, IntPtr.Zero);
            commands = new ComputeCommandQueue(context, Device, ComputeCommandQueueFlags.None);
            //events = new Collection<ComputeEventBase>();
            events = null;

            // I am specifying the first device, the original example did not, but it does not make a difference in performance.
            kernelFirst = program.CreateKernel("firstrun");
            kernelSecond = program.CreateKernel("secondrun");
            kernelG = program.CreateKernel("prepareMatrixG");
            kernelFr = program.CreateKernel("prepareFr");
            kernelFFl = program.CreateKernel("prepareFFl");
            kernelFl = program.CreateKernel("prepareFl");
            kernelFFr = program.CreateKernel("prepareFFr");
            kernelBFirst = program.CreateKernel("prepareBFirst");
            kernelBSecond = program.CreateKernel("prepareBSecond");

        }

        public void SetArgumentsToKernels()
        {
            //commonProperties 
            int maxI = I + 1;
            int maxJ = J + 1;
            //IJ
            int[] IJ = new int[3];
            IJ[0] = I;
            IJ[1] = J;

            //params G
            float[] paramsG = new float[6];
            paramsG[0] = hr;
            paramsG[1] = hz;
            paramsG[2] = ht;
            paramsG[3] = a;
            paramsG[4] = P;
            paramsG[5] = beta;

            //params R
            float[] paramsR = new float[3];
            paramsR[0] = gamma;
            paramsR[1] = sigm;
            paramsR[2] = c;

            //params L
            float[] paramsL = new float[3];
            paramsL[0] = gammaZ;
            paramsL[1] = sigmZ;
            paramsL[2] = c;

            IGCL = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            FrACl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[I + 1]);
            FrBCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[I]);
            FrCCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[I]);

            FFlACl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[J + 1]);
            FFlBCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[J]);
            FFlCCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[J]);

            FCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[maxI * maxJ]);
            tempLayerCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[maxI * maxJ]);

            paramsRCl = new ComputeBuffer<float>(kernelFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsR);
            paramsLCl = new ComputeBuffer<float>(kernelFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsL);
            paramsGCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsG);

            GCl = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, new float[maxI * maxJ]);

            //kernelFirst

            kernelFirst.SetMemoryArgument(0, IGCL);
            kernelFirst.SetMemoryArgument(1, FrACl);
            kernelFirst.SetLocalArgument(2, maxI*sizeof(float));
            kernelFirst.SetMemoryArgument(3, FrBCl);
            kernelFirst.SetMemoryArgument(4, FrCCl);
            kernelFirst.SetMemoryArgument(5, FCl);
            kernelFirst.SetMemoryArgument(6, tempLayerCl);

            //kernelSecond

            kernelSecond.SetMemoryArgument(0, IGCL);
            kernelSecond.SetMemoryArgument(1, FFlACl);
            kernelSecond.SetLocalArgument(2, maxJ * sizeof(float));
            kernelSecond.SetMemoryArgument(3, FFlBCl);
            kernelSecond.SetMemoryArgument(4, FFlCCl);
            kernelSecond.SetMemoryArgument(5, FCl);
            kernelSecond.SetMemoryArgument(6, tempLayerCl);

            //kernelG
            kernelG.SetMemoryArgument(0, IGCL);
            kernelG.SetMemoryArgument(1, paramsGCl);
            kernelG.SetMemoryArgument(2, GCl);

            //kernelFr
            kernelFr.SetMemoryArgument(0, IGCL);
            kernelFr.SetMemoryArgument(1, paramsRCl);
            kernelFr.SetMemoryArgument(2, FrACl);
            kernelFr.SetMemoryArgument(3, FrBCl);
            kernelFr.SetMemoryArgument(4, FrCCl);

            //kernelFFr
            kernelFFr.SetMemoryArgument(0, IGCL);
            kernelFFr.SetMemoryArgument(1, paramsRCl);
            kernelFFr.SetMemoryArgument(2, tempLayerCl);
            kernelFFr.SetMemoryArgument(3, FCl);

            //kernelBFirst
            kernelBFirst.SetMemoryArgument(0, IGCL);
            kernelBFirst.SetMemoryArgument(1, FCl);
            kernelBFirst.SetMemoryArgument(2, GCl);

            //kernelBSecond
            kernelBSecond.SetMemoryArgument(0, IGCL);
            kernelBSecond.SetMemoryArgument(1, FCl);
            kernelBSecond.SetMemoryArgument(2, GCl);

            //kernelFl
            kernelFl.SetMemoryArgument(0, IGCL);
            kernelFl.SetMemoryArgument(1, paramsLCl);
            kernelFl.SetMemoryArgument(2, tempLayerCl);
            kernelFl.SetMemoryArgument(3, FCl);

            //kernelFFl
            kernelFFl.SetMemoryArgument(0, IGCL);
            kernelFFl.SetMemoryArgument(1, paramsLCl);
            kernelFFl.SetMemoryArgument(2, FFlACl);
            kernelFFl.SetMemoryArgument(3, FFlBCl);
            kernelFFl.SetMemoryArgument(4, FFlCCl);
        }

        public void CleanupUnusedKernels()
        {

        }

        public void CleanupPrograms()
        {
            // Empty catches are just for testing purposes.
            kernelFirst.Dispose();
            kernelSecond.Dispose();
            kernelFFr.Dispose();
            kernelFl.Dispose();
            kernelBFirst.Dispose();
            kernelBSecond.Dispose();
            kernelG.Dispose();
            kernelFr.Dispose();
            kernelFFl.Dispose();
            program.Dispose();
            context.Dispose();
        }

        public void runFirstKernel()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelFirst == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFirst, null, new long[] { maxJ }, localWorkersJ, events);
        }

        public float[,] runSecondKernel()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelSecond == null)
            {
                throw new Exception("Call InitPrograms first!");
            }
            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelSecond, null, new long[] { maxI }, localWorkersI, events);
            float[] retVal = commands.Read<float>(tempLayerCl, events);
            return MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);
        }

        public void prepareMatrixGCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelG == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelG, null, new long[] { maxI, maxJ }, localWorkersIJ, events);
        }

        public void prepareFrCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelFr == null)
            {
                throw new Exception("Call InitPrograms first!");
            }
            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFr, null, new long[] { 3 * maxI - 2 }, localWorkers3I, events);

            FrA = commands.Read<float>(FrACl, events);
        }

        public void prepareFFrCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelFFr == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFFr, null, new long[] { maxI, maxJ }, localWorkersIJ, events);
        }

        public void prepareBFirstCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelBFirst == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelBFirst, null, new long[] { maxI, maxJ }, localWorkersIJ, events);
        }

        public void prepareBSecondCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelBSecond == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelBSecond, null, new long[] { maxI, maxJ }, localWorkersIJ, events);
        }

        public void prepareFlCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            if (kernelFl == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFl, null, new long[] { maxI, maxJ }, localWorkersIJ, events);

        }


        public void prepareFFlCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFFl, null, new long[] { 3 * maxJ - 2 }, localWorkers3J, events);

            FFlA = commands.Read<float>(FFlACl, events);
        }

        public override void executeAlg()
        {
            int maxI = I + 1;
            int maxJ = J + 1;
            prepareMatrixGCl();
            //MatrixWriter.writeMatrixToFile("G CL", MatrixHelper.VectorToMatrix(commands.Read<float>(GCl, events),ref maxI, ref maxJ), I + 1, J + 1);
            prepareFrCl();
            //MatrixWriter.writeVectorAsString("FrA CL", commands.Read<float>(FrACl, null), I + 1,true);
            //MatrixWriter.writeVectorAsString("FrB CL", commands.Read<float>(FrBCl, events), I, true);
            //MatrixWriter.writeVectorAsString("FrC CL", commands.Read<float>(FrCCl, null), I, true);
            prepareFFlCl();
            //MatrixWriter.writeVectorAsString("FFlA CL", commands.Read<float>(FFlACl, events), J + 1,true);
            //MatrixWriter.writeVectorAsString("FFlB CL", commands.Read<float>(FFlBCl, events), J, true);
            //MatrixWriter.writeVectorAsString("FFlC CL", commands.Read<float>(FFlCCl, events), J, true);
            for (int n = 0; n <= N - 1; n++)
            {


                prepareFlCl();
                //if (n == 1) MatrixWriter.writeMatrixToFile("Fl CL n=" + n, MatrixHelper.VectorToMatrix(commands.Read<float>(FCl, events), ref maxI, ref maxJ), maxI, maxJ);
                prepareBFirstCl();
                //if (n == 0) MatrixWriter.writeMatrixToFile("B1 CL n=" + n, MatrixHelper.VectorToMatrix(commands.Read<float>(FCl, events), ref maxI, ref maxJ), maxI, maxJ);

                runFirstKernel();
                //if (n == 0) MatrixWriter.writeMatrixToFile("tempLayer1 CL n=" + n, MatrixHelper.VectorToMatrix(commands.Read<float>(tempLayerCl, events), ref maxI, ref maxJ), maxI, maxJ);
                prepareFFrCl();
                //MatrixWriter.writeMatrixToFile("FFr CL n=" + n, Fl, I + 1, J + 1);
                prepareBSecondCl();
                //MatrixWriter.writeMatrixToFile("B2 CL n=" + n, B, I + 1, J + 1);
                tempLayer = runSecondKernel();
                //MatrixWriter.writeMatrixToFile("tempLayer2 CL n=" + n, tempLayer, I + 1, J + 1);

                /*commands.Write<float>(FrACl, FrA, events);
                commands.Write<float>(FFlACl, FFlA, events);*/

                copyToProc(tempLayer, n + 1);
            }
        }

        public override void executeProcess()
        {
            //execute();
            swInit.Start();
            initValues();
            executeOpenCLKernel();
            isExecuted = true;

        }

        public override void executeProcess(object parameters)
        {
            //execute();
            swInit.Start();
            initValues();
            handler = (DiplomWPF.ProcessControl.increaseProgressBar)parameters;
            executeOpenCLKernel();
            isExecuted = true;


        }

        protected void executeOpenCLKernel()
        {
            try
            {
                
                System.IO.File.WriteAllText("logCL.txt", "");
                InitPrograms();
                SetArgumentsToKernels();
                findOptimalLocalWorkers();
                swInit.Stop(); swCompute.Start();
                executeAlg();
                swCompute.Stop();
                CleanupPrograms();
            }
            catch (ComputeException exception)
            {
                List<string> lineList = new List<string>();
                foreach (ComputeDevice device in context.Devices)
                {
                    string header = "PLATFORM: " + context.Platform.Name + ", DEVICE: " + device.Name;
                    lineList.Add(header);

                    StringReader reader = new StringReader(program.GetBuildLog(device));
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        lineList.Add(line);
                        line = reader.ReadLine();
                    }

                    lineList.Add("");
                    lineList.Add(exception.Message);
                }
                System.IO.File.AppendAllLines("logCL.txt", lineList.ToArray());
            }


        }



    }
}

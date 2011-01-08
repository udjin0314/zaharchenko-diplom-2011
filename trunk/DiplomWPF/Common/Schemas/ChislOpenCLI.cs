using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Cloo;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;

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
        private ComputeKernel kernelB;
        private ComputeKernel kernelFFr;

        private float[,] Gsh;

        private float[] FrA;
        private float[] FrB;
        private float[] FrC;

        private float[] FFlA;
        private float[] FFlB;
        private float[] FFlC;

        private Boolean fi = true;



        public ChislOpenCLI(String name, Brush brush)
            : base(name, brush)
        {
        }

        protected void initComputeContext()
        {
            ComputePlatform Platform = ComputePlatform.Platforms[0];
            ComputeContextPropertyList properties = new ComputeContextPropertyList(Platform);
            context = new ComputeContext(Platform.Devices, properties, null, IntPtr.Zero);
        }

        public void InitPrograms()
        {
            ComputeContextPropertyList cpl = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);

            // This was ComputeDeviceTypes.Default, I tried Gpu instead with no noticeable difference.
            context = new ComputeContext(ComputeDeviceTypes.Gpu, cpl, null, IntPtr.Zero);

            program = new ComputeProgram(context, new[] { System.IO.File.ReadAllText(KERNEL_FILE) });

            commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            events = new Collection<ComputeEventBase>();

            // I am specifying the first device, the original example did not, but it does not make a difference in performance.
            program.Build(new[] { context.Devices[0] }, null, null, IntPtr.Zero);

            kernelFirst = program.CreateKernel("firstrun");
            kernelSecond = program.CreateKernel("secondrun");
            kernelG = program.CreateKernel("prepareMatrixG");
            kernelFr = program.CreateKernel("prepareFr");
            kernelFFl = program.CreateKernel("prepareFFl");
            kernelFl = program.CreateKernel("prepareFl");
            kernelFFr = program.CreateKernel("prepareFFr");
            kernelB = program.CreateKernel("prepareB");

        }

        public void CleanupUnusedKernels()
        {
            kernelG.Dispose();
            kernelFr.Dispose();
            kernelFFl.Dispose();
        }

        public void CleanupPrograms()
        {
            // Empty catches are just for testing purposes.
            kernelFirst.Dispose();
            kernelSecond.Dispose();
            kernelFFr.Dispose();
            kernelFl.Dispose();
            kernelB.Dispose();
            program.Dispose();
            context.Dispose();
        }

        public float[,] runKernel(ComputeKernel kernel, float[] a, float[] b, float[] c, float[,] B, float[,] tempLayer, int PrSize, int workers)
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] BVect = MatrixHelper.MatrixToVector(B, ref maxI, ref maxJ);
            float[] tempLayerVect = MatrixHelper.MatrixToVector(tempLayer, ref maxI, ref maxJ);


            if (kernel == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> aCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, a);
            ComputeBuffer<float> bCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, b);
            ComputeBuffer<float> cCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, c);
            ComputeBuffer<float> Bcl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, BVect);
            ComputeBuffer<float> tempLayerCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, tempLayerVect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernel.SetMemoryArgument(0, IGCL);
            kernel.SetMemoryArgument(1, aCl);
            kernel.SetMemoryArgument(2, bCl);
            kernel.SetMemoryArgument(3, cCl);
            kernel.SetMemoryArgument(4, Bcl);
            kernel.SetMemoryArgument(5, tempLayerCl);

           

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernel, null, new long[] { workers }, null, null);
            //commands.Execute(kernel, null, new long[] { count }, null, null);

            float[] retVal = new float[tempLayerVect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(tempLayerCl, true, 0, tempLayerVect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            tempLayer = MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);

            return tempLayer;
        }

        public float[,] prepareMatrixGCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] paramsV = new float[6];
            paramsV[0] = hr;
            paramsV[1] = hz;
            paramsV[2] = ht;
            paramsV[3] = a;
            paramsV[4] = P;
            paramsV[5] = beta;

            Gsh = MatrixHelper.getStdMatrix(I + 1, J + 1);

            float[] Avect = new float[maxI*maxJ];

            if (kernelG == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> paramsCl = new ComputeBuffer<float>(kernelG.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsV);
            ComputeBuffer<float> ACl = new ComputeBuffer<float>(kernelG.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, Avect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelG.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelG.SetMemoryArgument(0, IGCL);
            kernelG.SetMemoryArgument(1, paramsCl);
            kernelG.SetMemoryArgument(2, ACl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelG, null, new long[] { maxI, maxJ }, new long[] { maxI, maxJ }, events);

            float[] retVal = new float[Avect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(ACl, true, 0, Avect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            Gsh = MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);

            return Gsh;
        }

        public void prepareFrCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            int workers = 3 * maxI - 2;

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] paramsV = new float[3];
            paramsV[0] = gamma;
            paramsV[1] = sigm;
            paramsV[2] = c;

            FrA = new float[maxI];
            FrB = new float[maxI - 1];
            FrC = new float[maxI - 1];

            float[] Avect = new float[workers];

            if (kernelFr == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> paramsCl = new ComputeBuffer<float>(kernelFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsV);
            ComputeBuffer<float> ACl = new ComputeBuffer<float>(kernelFr.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, Avect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelFr.SetMemoryArgument(0, IGCL);
            kernelFr.SetMemoryArgument(1, paramsCl);
            kernelFr.SetMemoryArgument(2, ACl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFr, null, new long[] { workers }, null, events);

            float[] retVal = new float[Avect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(ACl, true, 0, Avect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            for (int i = 0; i < maxI - 1; i++)
                FrC[i] = retVal[i];

            for (int i = maxI - 1; i < 2 * maxI - 1; i++)
                FrA[i - maxI + 1] = retVal[i];

            for (int i = 2 * maxI - 1; i < 3 * maxI - 2; i++)
                FrB[i - 2 * maxI + 1] = retVal[i];
        }

        public float[,] prepareFFrCl(float[,] neededLayer)
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            float[,] FFr = MatrixHelper.getStdMatrix(maxI, maxJ);

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] paramsV = new float[3];
            paramsV[0] = gamma;
            paramsV[1] = sigm;
            paramsV[2] = c;

            float[] FFrVect = new float[maxI*maxJ];
            float[] neededLayerVect = MatrixHelper.MatrixToVector(neededLayer, ref maxI, ref maxJ);

            if (kernelFFr == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> paramsCl = new ComputeBuffer<float>(kernelFFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsV);
            ComputeBuffer<float> neededLayerCl = new ComputeBuffer<float>(kernelFFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, neededLayerVect);
            ComputeBuffer<float> FFrCl = new ComputeBuffer<float>(kernelFFr.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, FFrVect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelFFr.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelFFr.SetMemoryArgument(0, IGCL);
            kernelFFr.SetMemoryArgument(1, paramsCl);
            kernelFFr.SetMemoryArgument(2, neededLayerCl);
            kernelFFr.SetMemoryArgument(3, FFrCl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFFr, null, new long[] { maxI, maxJ }, new long[] { maxI, maxJ }, events);

            float[] retVal = new float[FFrVect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(FFrCl, true, 0, FFrVect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            FFr = MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);

            return FFr;
        }

        public float[,] prepareBCl(float[,] A1, float[,] A2, int koef)
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            float[,] B = MatrixHelper.getStdMatrix(maxI, maxJ);

            int[] IJ = new int[3];
            IJ[0] = I;
            IJ[1] = J;
            IJ[2] = koef;


            float[] A1Vect = MatrixHelper.MatrixToVector(A1, ref maxI, ref maxJ);
            float[] A2Vect = MatrixHelper.MatrixToVector(A2, ref maxI, ref maxJ);
            float[] BVect = new float[maxI*maxJ];

            if (kernelB == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> A1Cl = new ComputeBuffer<float>(kernelB.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, A1Vect);
            ComputeBuffer<float> A2Cl = new ComputeBuffer<float>(kernelB.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, A2Vect);
            ComputeBuffer<float> BCl = new ComputeBuffer<float>(kernelB.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, BVect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelB.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelB.SetMemoryArgument(0, IGCL);
            kernelB.SetMemoryArgument(1, A1Cl);
            kernelB.SetMemoryArgument(2, A2Cl);
            kernelB.SetMemoryArgument(3, BCl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelB, null, new long[] { maxI, maxJ }, new long[] { maxI, maxJ }, events);

            float[] retVal = new float[BVect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(BCl, true, 0, BVect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            B = MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);

            return B;
        }

        public float[,] prepareFlCl(float[,] neededLayer)
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            float[,] Fl = MatrixHelper.getStdMatrix(maxI, maxJ);

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] paramsV = new float[3];
            paramsV[0] = gammaZ;
            paramsV[1] = sigmZ;
            paramsV[2] = c;

            float[] FlVect = new float[maxI*maxJ];
            float[] neededLayerVect = MatrixHelper.MatrixToVector(neededLayer, ref maxI, ref maxJ);

            if (kernelFl == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> paramsCl = new ComputeBuffer<float>(kernelFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsV);
            ComputeBuffer<float> neededLayerCl = new ComputeBuffer<float>(kernelFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, neededLayerVect);
            ComputeBuffer<float> FlCl = new ComputeBuffer<float>(kernelFl.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, FlVect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelFl.SetMemoryArgument(0, IGCL);
            kernelFl.SetMemoryArgument(1, paramsCl);
            kernelFl.SetMemoryArgument(2, neededLayerCl);
            kernelFl.SetMemoryArgument(3, FlCl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFl, null, new long[] { maxI, maxJ }, new long[] { maxI, maxJ }, events);

            float[] retVal = new float[FlVect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(FlCl, true, 0, FlVect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            Fl = MatrixHelper.VectorToMatrix(retVal, ref maxI, ref maxJ);

            return Fl;
        }


        public void prepareFFlCl()
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            int workers = 3 * maxJ - 2;

            FFlA = new float[maxJ];
            FFlB = new float[maxJ - 1];
            FFlC = new float[maxJ - 1];

            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;

            float[] paramsV = new float[3];
            paramsV[0] = gammaZ;
            paramsV[1] = sigmZ;
            paramsV[2] = c;


            float[] Avect = new float[workers];

            if (kernelFFl == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> paramsCl = new ComputeBuffer<float>(kernelFFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, paramsV);
            ComputeBuffer<float> ACl = new ComputeBuffer<float>(kernelFFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, Avect);
            ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernelFFl.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, IJ);

            kernelFFl.SetMemoryArgument(0, IGCL);
            kernelFFl.SetMemoryArgument(1, paramsCl);
            kernelFFl.SetMemoryArgument(2, ACl);


            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernelFFl, null, new long[] { workers }, null, events);

            float[] retVal = new float[Avect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(ACl, true, 0, Avect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            for (int i = 0; i < maxJ - 1; i++)
                FFlC[i] = retVal[i];

            for (int i = maxJ - 1; i < 2 * maxJ - 1; i++)
                FFlA[i - maxJ + 1] = retVal[i];

            for (int i = 2 * maxJ - 1; i < 3 * maxJ - 2; i++)
                FFlB[i - 2 * maxJ + 1] = retVal[i];
        }


        private void initializeVarMatrixes()
        {
            Gsh = prepareMatrixGCl();
            tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            prepareFrCl();
            prepareFFlCl();
        }

        public override void executeAlg()
        {

            for (int n = 0; n <= N - 1; n++)
            {
                float[,] Fl = prepareFlCl(tempLayer);
                float[,] B = prepareBCl(Fl, Gsh, -1);


                tempLayer = runKernel(kernelFirst, FrA, FrB, FrC, B, tempLayer, I + 1, J + 1);
                //tempLayer = firstrunTest(Fr, B, tempLayer, I + 1, J + 1);

                Fl = prepareFFrCl(tempLayer);
                B = prepareBCl(Fl, Gsh, 1);
                tempLayer = runKernel(kernelSecond, FFlA, FFlB, FFlC, B, tempLayer, J + 1, I + 1);
                //tempLayer = secondrunTest(FFl, B, tempLayer, J + 1, I + 1);


                copyToProc(tempLayer, n + 1);
            }
        }

        public override void executeProcess()
        {
            //execute();
            values = new ProcessValues(Ipv + 1, Jpv + 1, Npv + 1);
            executeOpenCLKernel();
            isExecuted = true;

        }

        public override void executeProcess(object parameters)
        {
            //execute();
            values = new ProcessValues(Ipv + 1, Jpv + 1, Npv + 1);
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
                initializeVarMatrixes();
                CleanupUnusedKernels();
                executeAlg();
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

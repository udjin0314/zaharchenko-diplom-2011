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
        private ComputeKernel kernelFirst;
        private ComputeKernel kernelSecond;
        private float[,] Gsh;
        private float[,] Fr;
        private float[,] FFl;




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

            // I am specifying the first device, the original example did not, but it does not make a difference in performance.
            program.Build(new[] { context.Devices[0] }, null, null, IntPtr.Zero);

            kernelFirst = program.CreateKernel("firstrun");
            kernelSecond = program.CreateKernel("secondrun");
        }

        public void CleanupPrograms()
        {
            // Empty catches are just for testing purposes.
            kernelFirst.Dispose();
            kernelSecond.Dispose();
            program.Dispose();
            context.Dispose();
        }



        public void initKernels()
        {
            initKernel(kernelFirst);
            initKernel(kernelSecond);
        }

        private void initKernel(ComputeKernel kernel)
        {
            if (kernel == null)
            {
                throw new Exception("Call InitPrograms first!");
            }
            int[] IJ = new int[2];
            IJ[0] = I;
            IJ[1] = J;
            using (ComputeBuffer<int> IGCL = new ComputeBuffer<int>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, IJ))
            {
                kernel.SetMemoryArgument(0, IGCL);
            }
        }

        public float[,] runKernel(ComputeKernel kernel, float[,] F, float[,] B, float[,] tempLayer, int PrSize, int workers)
        {
            int maxI = I + 1;
            int maxJ = J + 1;

            float[] FClVect = MatrixHelper.MatrixToVector(F, ref maxI, ref maxJ);
            float[] BVect = MatrixHelper.MatrixToVector(B, ref maxI, ref maxJ);
            float[] tempLayerVect = MatrixHelper.MatrixToVector(tempLayer, ref maxI, ref maxJ);


            if (kernel == null)
            {
                throw new Exception("Call InitPrograms first!");
            }

            ComputeBuffer<float> FCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, FClVect);
            ComputeBuffer<float> Bcl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, BVect);
            ComputeBuffer<float> tempLayerCl = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, tempLayerVect);

            kernel.SetMemoryArgument(1, FCl);
            kernel.SetMemoryArgument(2, Bcl);
            kernel.SetMemoryArgument(3, tempLayerCl);
            kernel.SetLocalArgument(4, PrSize);
            kernel.SetLocalArgument(5, PrSize);

            ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            ICollection<ComputeEventBase> events = new Collection<ComputeEventBase>();

            // BUG: ATI Stream v2.2 crash if event list not null.
            commands.Execute(kernel, null, new long[] { workers }, null, events);
            //commands.Execute(kernel, null, new long[] { count }, null, null);

            float[] retVal = new float[tempLayerVect.Length];
            GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);


            commands.Read(tempLayerCl, true, 0, tempLayerVect.Length, arrCHandle.AddrOfPinnedObject(), events);

            arrCHandle.Free();

            tempLayer = MatrixHelper.VectorToMatrix(tempLayerVect, ref maxI, ref maxJ);

            return tempLayer;

            /*using (ComputeCommandQueue queue = new ComputeCommandQueue(kernel.Context,
                                                                kernel.Context.Devices[0],
                                                                ComputeCommandQueueFlags.None))
            {
                queue.Execute(kernel, null, new long[] { vec1.Length }, null, null);
                float[] retVal = queue.Read(c, true, 0, vec1.Length, null, new Collection<ComputeEventBase>());
                return retVal;




            }*/
        }

        private void initializeVarMatrixes()
        {
            Gsh = prepareMatrixG();
            tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            Fr = prepareFr();
            FFl = prepareFFl();
        }

        public override void executeAlg()
        {

            for (int n = 0; n <= N - 1; n++)
            {
                float[,] Fl = prepareFl(tempLayer);
                float[,] B = prepareB(Fl, Gsh, -1);

                tempLayer = runKernel(kernelFirst, Fl, B, tempLayer, I + 1, J + 1);

                Fl = prepareFFr(tempLayer);
                B = prepareB(Fl, Gsh, 1);
                tempLayer = runKernel(kernelSecond, Fl, B, tempLayer, J + 1, I + 1);

                copyToProc(tempLayer, n + 1);
            }
        }

        public override void executeProcess()
        {
            //execute();
            values = new ProcessValues(I, J, N);
            executeOpenCLKernel();
            isExecuted = true;

        }

        public override void executeProcess(object parameters)
        {
            //execute();
            values = new ProcessValues(I, J, N);
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
                initKernels();
                executeAlg();
                CleanupPrograms();
            }
            catch (Exception exception)
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

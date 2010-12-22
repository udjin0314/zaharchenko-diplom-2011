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
    class ChislOpenCLI : AbstractProcess
    {
        public static String KERNEL_FILE = "Common/Schemas/OpenCL/PismenRekford.cpp";
        private ComputeContext context;
        private ComputeProgram program;
        private ComputeKernel kernel;

        public static String FILE_NAME = "log.txt";
        private float gammaZ = 0;
        private float gamma = 0;
        private float sigm = 0;
        private float sigmZ = 0;
        protected float[,] tempLayer;

        public ChislOpenCLI(String name, Brush brush)
            : base(name, brush)
        {
        }

        public override void initialize(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T, Int32 N, Int32 I, Int32 J)
        {
            base.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
            gamma = ht * K / (2 * hr * hr);
            gammaZ = ht * K / (2 * hz * hz);
            sigm = 2 * gamma * (1 + (1 + (float)1 / (2 * I)) * hr * alphaR / K);
            sigmZ = 2 * gammaZ * (hz * alphaZ / K + 1);
        }

        public override void initializeParams(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T)
        {
            base.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
            gamma = ht * K / (2 * hr * hr);
            gammaZ = ht * K / (2 * hz * hz);
            sigm = 2 * gamma * (1 + (1 + (float)1 / (2 * I)) * hr * alphaR / K);
            sigmZ = 2 * gammaZ * (hz * alphaZ / K + 1);
        }

        public virtual void executeAlg()
        {
            executeOpenCLKernel();

            float[,] Gsh = prepareMatrixG();
            tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            float[,] Fr = prepareFr();
            float[,] FFl = prepareFFl();
            for (int n = 0; n <= N - 1; n++)
            {
                float[,] Fl = prepareFl(tempLayer);
                float[,] B = prepareB(Fl, Gsh, -1);
                for (int j = 0; j <= J; j++)
                {
                    float[] Bloc = MatrixHelper.getCol(B, j, I + 1);
                    float[] Prloc = MatrixHelper.progonka(Fr, Bloc, I + 1);

                    MatrixHelper.setCol(tempLayer, Prloc, j, I + 1);
                }

                Fl = prepareFFr(tempLayer);
                B = prepareB(Fl, Gsh, 1);

                for (int i = 0; i <= I; i++)
                {
                    float[] Bloc = MatrixHelper.getRow(B, i, J + 1);
                    float[] Prloc = MatrixHelper.progonka(FFl, Bloc, J + 1);
                    MatrixHelper.setRow(tempLayer, Prloc, i, J + 1);

                }
                copyToProc(tempLayer, n + 1);
            }
        }

        public override void executeProcess()
        {
            //execute();
            base.executeProcess();
            executeAlg();
            isExecuted = true;

        }

        public override void executeProcess(object parameters)
        {
            //execute();
            base.executeProcess(parameters);
            executeAlg();
            isExecuted = true;


        }



        protected float functionG(int i, int j)
        {
            float r = i * hr;
            float z = j * hz;
            float res = (float)(P * beta / (Math.PI * a * a) * Math.Exp(-(beta * z + (r * r / (a * a)))));
            return res;
        }



        protected float[,] prepareMatrixG()
        {
            float[,] A = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int j = 0; j <= J; j++)
                for (int i = 0; i <= I; i++)
                    A[i, j] = 0.5F * ht * functionG(i, j);
            return A;
        }





        protected float[,] prepareB(float[,] A1, float[,] A2, int koef)
        {
            float[,] B = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
                for (int j = 0; j <= J; j++)
                    B[i, j] = koef * (A1[i, j] + A2[i, j]);
            return B;
        }

        protected float[,] prepareFr()
        {
            float[,] Fr = MatrixHelper.getStdMatrix(I + 1, I + 1);
            Fr[0, 0] = -(4 * gamma + c);
            Fr[0, 1] = 4 * gamma;
            Fr[I, I - 1] = 2 * gamma;
            Fr[I, I] = -(sigm + c);

            for (int i = 1; i <= I - 1; i++)
            {
                Fr[i, i - 1] = gamma * (float)(1 - (float)1 / (2 * i));
                Fr[i, i] = -(2 * gamma + c);
                Fr[i, i + 1] = gamma * (float)(1 + (float)1 / (2 * i));

            }
            return Fr;

        }

        protected float[,] prepareFl(float[,] neededLayer)
        {
            float[,] Fl = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
            {
                Fl[i, 0] = (c - sigmZ) * neededLayer[i, 0] + 2 * gammaZ * (neededLayer[i, 1]);
            }

            for (int i = 0; i <= I; i++)
            {
                Fl[i, J] = (c - sigmZ) * neededLayer[i, J] + 2 * gammaZ * (neededLayer[i, J - 1]);
            }

            for (int j = 1; j <= J - 1; j++)
                for (int i = 0; i <= I; i++)
                {
                    Fl[i, j] = gammaZ * neededLayer[i, j - 1] + (c - 2 * gammaZ) * neededLayer[i, j] + gammaZ * neededLayer[i, j + 1];
                }
            return Fl;
        }

        protected float[,] prepareFFr(float[,] neededLayer)
        {
            float[,] Fl = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int j = 0; j <= J; j++)
            {
                Fl[0, j] = (c - 4 * gamma) * (neededLayer[0, j]) + 4 * gamma * (neededLayer[1, j]);
            }

            for (int j = 0; j <= J; j++)
            {
                Fl[I, j] = (2 * gamma) * (neededLayer[I - 1, j]) - (sigm - c) * (neededLayer[I, j]);
            }

            for (int i = 1; i <= I - 1; i++)
                for (int j = 0; j <= J; j++)
                {
                    Fl[i, j] = gamma * (1 - (float)1 / (2 * i)) * neededLayer[i - 1, j] - (2 * gamma - c) * neededLayer[i, j] + gamma * (1 + (float)1 / (2 * i)) * neededLayer[i + 1, j];
                }
            return Fl;
        }

        protected float[,] prepareFFl()
        {
            float[,] Fr = MatrixHelper.getStdMatrix(J + 1, J + 1);
            Fr[0, 0] = (c + sigmZ);
            Fr[0, 1] = -2 * gammaZ;
            Fr[J, J - 1] = -2 * gammaZ;
            Fr[J, J] = (c + sigmZ);

            for (int i = 1; i <= J - 1; i++)
            {
                Fr[i, i - 1] = -gammaZ;
                Fr[i, i] = (2 * gammaZ + c);
                Fr[i, i + 1] = -gammaZ;
            }
            return Fr;

        }

        protected void copyToProc(float[,] res, int n)
        {
            for (int j = 0; j <= J; j++)
                for (int i = 0; i <= I; i++)
                {
                    values[i, j, n] = res[i, j];
                    if (res[i, j] > maxTemperature)
                        maxTemperature = res[i, j];
                    if (res[i, j] < minTemperature)
                        minTemperature = res[i, j];
                }
            if (handler != null) handler();

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

            kernel = program.CreateKernel("VectorAdd");
        }

        public void CleanupPrograms()
        {
            // Empty catches are just for testing purposes.
            kernel.Dispose();
            program.Dispose();
            context.Dispose();
        }

        public float[] procOpenCL(float[] vec1, float[] vec2)
        {
            if (kernel == null)
            {
                throw new Exception("Call InitPrograms first!");
            }
            using (ComputeBuffer<float> a = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, vec1))
            {
                using (ComputeBuffer<float> b = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, vec2))
                {
                    using (ComputeBuffer<float> c = new ComputeBuffer<float>(kernel.Context, ComputeMemoryFlags.WriteOnly, vec1.Length))
                    {
                        kernel.SetMemoryArgument(0, a);
                        kernel.SetMemoryArgument(1, b);
                        kernel.SetMemoryArgument(2, c);
                        ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

                        ICollection<ComputeEventBase> events = new Collection<ComputeEventBase>();

                        // BUG: ATI Stream v2.2 crash if event list not null.
                        commands.Execute(kernel, null, new long[] { vec1.Length }, null, events);
                        //commands.Execute(kernel, null, new long[] { count }, null, null);

                        float[] retVal = new float[vec1.Length];
                        GCHandle arrCHandle = GCHandle.Alloc(retVal, GCHandleType.Pinned);

                        commands.Read(c, true, 0, vec1.Length, arrCHandle.AddrOfPinnedObject(), events);

                        arrCHandle.Free();

                        return retVal;

                        /*using (ComputeCommandQueue queue = new ComputeCommandQueue(kernel.Context,
                                                                            kernel.Context.Devices[0],
                                                                            ComputeCommandQueueFlags.None))
                        {
                            queue.Execute(kernel, null, new long[] { vec1.Length }, null, null);
                            float[] retVal = queue.Read(c, true, 0, vec1.Length, null, new Collection<ComputeEventBase>());
                            return retVal;




                        }*/
                    }
                }
            }
        }

        protected void executeOpenCLKernel()
        {
            try
            {
                System.IO.File.WriteAllText("logCL.txt", "");
                InitPrograms();
                for (float x = 0; x <= 24; x++)
                {
                    for (float y = 0; y < 100; y++)
                    {
                        float[] c = procOpenCL(new[] { x, y }, new[] { x, y });
                        System.IO.File.AppendAllText("logCL.txt", "(" + c[0] + "; " + c[1] + "), ");
                    }
                }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using DiplomWPF.Common.Helpers;
using System.Threading;

namespace DiplomWPF.Common.Schemas
{
    class ParallelChislSchema : ChislProcess
    {

        private int threadsN = 4;

        public ParallelChislSchema(String name, Brush brush)
            : base(name, brush)
        {
        }

        protected void firstrun(object parameters)
        {
            Dictionary<String, Object> prms = (Dictionary<String, Object>)parameters;
            float[,] B = (float[,])prms["B"];
            int j1 = (int)prms["j1"];
            int j2 = (int)prms["j2"];
            float[,] Fr = (float[,])prms["Fr"];
            for (int j = j1; j <= j2; j++)
            {
                float[] Bloc = MatrixHelper.getCol(B, j, I + 1);
                float[] Prloc = MatrixHelper.progonka(Fr, Bloc, I + 1);
                lock (tempLayer)
                {
                    MatrixHelper.setCol(tempLayer, Prloc, j, I + 1);
                }
            }
        }

        protected void secondrun(object parameters)
        {
            Dictionary<String, Object> prms = (Dictionary<String, Object>)parameters;
            float[,] B = (float[,])prms["B"];
            int i1 = (int)prms["i1"];
            int i2 = (int)prms["i2"];
            float[,] FFl = (float[,])prms["FFl"];
            for (int i = i1; i <= i2; i++)
            {
                float[] Bloc = MatrixHelper.getRow(B, i, J + 1);
                float[] Prloc = MatrixHelper.progonka(FFl, Bloc, J + 1);
                lock (tempLayer)
                {
                    MatrixHelper.setRow(tempLayer, Prloc, i, J + 1);
                }
            }
        }

        public override void executeAlg()
        {
            float[,] Gsh = prepareMatrixG();
            tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            float[,] Fr = prepareFr();
            float[,] FFl = prepareFFl();
            for (int n = 0; n <= N - 1; n++)
            {
                float[,] Fl = prepareFl(tempLayer);
                float[,] B = prepareB(Fl, Gsh, -1);
                DThreadPool pool = new DThreadPool();
                threadsN = J / 100;
                if (threadsN == 0) threadsN = 1;
                int intervalJ = J / threadsN;
                //for (int j = 0; j <= J; j++)
                for (int j = 0; j < threadsN; j++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(firstrun));

                    Dictionary<String, Object> firstParams = new Dictionary<string, object>();
                    firstParams.Add("Fr", Fr.Clone());
                    firstParams.Add("B", B.Clone());
                    if (j == 0) firstParams.Add("j1", j * intervalJ);
                    else firstParams.Add("j1", j * intervalJ + 1);
                    if (j == threadsN - 1) firstParams.Add("j2", J);
                    else firstParams.Add("j2", (j + 1) * intervalJ);
                    thread.Name = "n=" + n + "; j=" + firstParams["j1"] + ":" + firstParams["j2"];
                    pool.addThread(thread);
                    thread.Start(firstParams);

                }
                while (!pool.allThreadsCompleted()) ;
                pool.Clear();

                Fl = prepareFFr(tempLayer);
                B = prepareB(Fl, Gsh, 1);
                threadsN = I / 100;
                if (threadsN == 0) threadsN = 1;
                int intervalI = I / threadsN;
                //for (int i = 0; i <= I; i++)
                for (int i = 0; i < threadsN; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(secondrun));
                    Dictionary<String, Object> secondParams = new Dictionary<string, object>();
                    secondParams.Add("FFl", FFl.Clone());
                    secondParams.Add("B", B.Clone());
                    if (i == 0) secondParams.Add("i1", i * intervalI);
                    else secondParams.Add("i1", i * intervalI + 1);
                    if (i == threadsN - 1) secondParams.Add("i2", I);
                    else secondParams.Add("i2", (i + 1) * intervalI);
                    thread.Name = "n=" + n + "; i=" + secondParams["i1"] + ":" + secondParams["i2"];
                    pool.addThread(thread);
                    thread.Start(secondParams);


                }
                while (!pool.allThreadsCompleted()) ;
                copyToProc(tempLayer, n + 1);
            }
        }
    }
}

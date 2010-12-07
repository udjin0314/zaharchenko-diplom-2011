﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.ServerSide;
using System.Windows.Media;
using System.Threading;
using DiplomWPF.Common.Helpers;

namespace DiplomWPF.Common
{
    class ChislProcess : AbstractProcess
    {
        public static String FILE_NAME = "log.txt";
        private int threadsN = 1;
        private float gammaZ = 0;
        private float gamma = 0;
        private float sigm = 0;
        private float sigmZ = 0;
        float[,] tempLayer;

        public ChislProcess(String name, Brush brush)
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

        private void firstrun(object parameters)
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

        private void secondrun(object parameters)
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


        public void executeAlg()
        {
            float[,] Gsh = prepareMatrixG();
            tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            float[,] Fr = prepareFr();
            float[,] FFl = prepareFFl();
            for (int n = 0; n <= N - 1; n++)
            {
                float[,] Fl = prepareFl(tempLayer);
                float[,] B = prepareB(Fl, Gsh, -1);
                DThreadPool pool = new DThreadPool(); ;
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
                    pool.addThread(thread);
                    thread.Start(firstParams);
                    /*float[] Bloc = MatrixHelper.getCol(B, j, I + 1);
                    float[] Prloc = MatrixHelper.progonka(Fr, Bloc, I + 1);

                    MatrixHelper.setCol(tempLayer, Prloc, j, I + 1);*/
                }
                while (!pool.allThreadsCompleted()) ;
                pool.Clear();

                Fl = prepareFFr(tempLayer);
                B = prepareB(Fl, Gsh, 1);

                int intervalI = I / threadsN;
                //for (int i = 0; i <= I; i++)
                for (int i = 0; i < threadsN; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(secondrun));
                    Dictionary<String, Object> secondParams = new Dictionary<string, object>();
                    secondParams.Add("FFl", FFl.Clone());
                    secondParams.Add("B", B.Clone());
                    if (i == 0) secondParams.Add("i1", i * intervalI);
                    else secondParams.Add("i1", i * intervalI+1);
                    if (i == threadsN - 1) secondParams.Add("i2", I);
                    else secondParams.Add("i2", (i + 1) * intervalI);
                    pool.addThread(thread);
                    thread.Start(secondParams);
                    /*float[] Bloc = MatrixHelper.getRow(B, i, J + 1);
                    float[] Prloc = MatrixHelper.progonka(FFl, Bloc, J + 1);
                    MatrixHelper.setRow(tempLayer, Prloc, i, J + 1);*/

                }
                while (!pool.allThreadsCompleted()) ;
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

        

        float functionG(int i, int j)
        {
            float r = i * hr;
            float z = j * hz;
            //TODO replace exponenta and pi
            float res = (float)(P * beta / (Math.PI * a * a) * Math.Exp(-(beta * z + (r * r / (a * a)))));
            //double res = P  / (Math.PI * a * a) * Math.Exp(- (r * r / (a * a)));
            return res;
        }



        float[,] prepareMatrixG()
        {
            float[,] A = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int j = 0; j <= J; j++)
                for (int i = 0; i <= I; i++)
                    A[i, j] = 0.5F * ht * functionG(i, j);
            return A;
        }





        float[,] prepareB(float[,] A1, float[,] A2, int koef)
        {
            float[,] B = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
                for (int j = 0; j <= J; j++)
                    B[i, j] = koef * (A1[i, j] + A2[i, j]);
            return B;
        }

        float[,] prepareFr()
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

        float[,] prepareFl(float[,] neededLayer)
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

        float[,] prepareFFr(float[,] neededLayer)
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

        float[,] prepareFFl()
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

        void copyToProc(float[,] res, int n)
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



    }
}

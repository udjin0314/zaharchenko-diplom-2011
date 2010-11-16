using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.ServerSide;
using System.Windows.Media;

namespace DiplomWPF.Common
{
    class ChislProcess : AbstractProcess
    {
        public static String FILE_NAME = "log.txt";

        private double gammaZ = 0;
        private double gamma = 0;
        private double sigm = 0;
        private double sigmZ = 0;

        public ChislProcess(String name, Brush brush)
            : base(name, brush)
        {
        }

        public override void initialize(Double P, Double alphaR, Double alphaZ, Double R, Double l, Double K, Double c, Double beta, Double T, Int32 N, Int32 I, Int32 J)
        {
            base.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
            gamma = ht * K / (2 * hr * hr);
            gammaZ = ht * K / (2 * hz * hz);
            sigm = 2 * gamma * (1 + (1 + (double)1 / (2 * I)) * hr * alphaR / K);
            sigmZ = 2 * gammaZ * (hz * alphaZ / K + 1);
        }

        public void execute()
        {
            ChislExecuter chislExecutor = new ChislExecuter();
            chislExecutor.getProcess(this);
        }

        public void executeAlg()
        {
            MatrixWriter.createFile(FILE_NAME);
            double[,] Gsh = prepareMatrixG();
            MatrixWriter.writeMatrixToFile(FILE_NAME, "G", Gsh, I + 1, J + 1, false);
            double[,] tempLayer = MatrixHelper.getStdMatrix(I + 1, J + 1);
            double[,] Fr = prepareFr();
            MatrixWriter.writeMatrixToFile(FILE_NAME, "Fr", Fr, I + 1, I + 1, false);
            double[,] FFl = prepareFFl();
            MatrixWriter.writeMatrixToFile(FILE_NAME, "FFl", FFl, J + 1, J + 1, false);
            for (int n = 0; n <= N - 1; n++)
            {
                double[,] Fl = prepareFl(tempLayer);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "Fl n=" + n, Fl, I + 1, J + 1, false);
                double[,] B = prepareB(Fl, Gsh, -1);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "Bl n=" + n, B, I + 1, J + 1, false);
                for (int j = 0; j <= J; j++)
                {
                    double[] Bloc = MatrixHelper.getCol(B, j, I + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Bloc j=" + j + " n=" + n, Bloc, I + 1, false);
                    double[] Prloc = MatrixHelper.progonka(Fr, Bloc, I + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Prloc j=" + j + " n=" + n, Prloc, I + 1, false);

                    MatrixHelper.setCol(tempLayer, Prloc, j, I + 1);
                    MatrixWriter.writeMatrixToFile(FILE_NAME, "u 1/2 n=" + n, tempLayer, I + 1, J + 1, false);
                }
                Fl = prepareFFr(tempLayer);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "Ffr n=" + n, Fl, I + 1, J + 1, false);
                B = prepareB(Fl, Gsh, 1);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "B2 n=" + n, B, I + 1, J + 1, false);
                for (int i = 0; i <= I; i++)
                {

                    double[] Bloc = MatrixHelper.getRow(B, i, J + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Bloc i=" + i + " n=" + n, Bloc, J + 1, true);
                    double[] Prloc = MatrixHelper.progonka(FFl, Bloc, J + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Prloc i=" + i + " n=" + n, Prloc, J + 1, true);
                    MatrixHelper.setRow(tempLayer, Prloc, i, J + 1);
                    MatrixWriter.writeMatrixToFile(FILE_NAME, "u 1/2 n=" + n, tempLayer, I + 1, J + 1, false);

                }
                MatrixWriter.writeMatrixToFile(FILE_NAME, "u n=" + (n + 1), tempLayer, I + 1, J + 1, false);
                copyToProc(tempLayer, n + 1);
            }
        }

        public override void executeProcess()
        {
            //execute();
            executeAlg();

        }

        double functionG(int i, int j)
        {
            double r = i * hr;
            double z = j * hz;
            //TODO replace exponenta and pi
            double res = P * beta / (Math.PI * a * a) * Math.Exp(-(beta * z + (r * r / (a * a))));
            //double res = P  / (Math.PI * a * a) * Math.Exp(- (r * r / (a * a)));
            return res;
        }



        double[,] prepareMatrixG()
        {
            double[,] A = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int j = 0; j <= J; j++)
                for (int i = 0; i <= I; i++)
                    A[i, j] = 0.5 * ht * functionG(i, j);
            return A;
        }





        double[,] prepareB(double[,] A1, double[,] A2, int koef)
        {
            double[,] B = MatrixHelper.getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
                for (int j = 0; j <= J; j++)
                    B[i, j] = koef * (A1[i, j] + A2[i, j]);
            return B;
        }

        double[,] prepareFr()
        {
            double[,] Fr = MatrixHelper.getStdMatrix(I + 1, I + 1);
            Fr[0, 0] = -(4 * gamma + c);
            Fr[0, 1] = 4 * gamma;
            Fr[I, I - 1] = 2 * gamma;
            Fr[I, I] = -(sigm + c);

            for (int i = 1; i <= I - 1; i++)
            {
                Fr[i, i - 1] = gamma * (double)(1 - (double)1 / (2 * i));
                Fr[i, i] = -(2 * gamma + c);
                Fr[i, i + 1] = gamma * (double)(1 + (double)1 / (2 * i));

            }
            return Fr;

        }

        double[,] prepareFl(double[,] neededLayer)
        {
            double[,] Fl = MatrixHelper.getStdMatrix(I + 1, J + 1);
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

        double[,] prepareFFr(double[,] neededLayer)
        {
            double[,] Fl = MatrixHelper.getStdMatrix(I + 1, J + 1);
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
                    Fl[i, j] = gamma * (1 - (double)1 / (2 * i)) * neededLayer[i - 1, j] - (2 * gamma - c) * neededLayer[i, j] + gamma * (1 + (double)1 / (2 * i)) * neededLayer[i + 1, j];
                }
            return Fl;
        }

        double[,] prepareFFl()
        {
            double[,] Fr = MatrixHelper.getStdMatrix(J + 1, J + 1);
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

        void copyToProc(double[,] res, int n)
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

        }



    }
}

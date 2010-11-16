using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common;

namespace DiplomWPF.ServerSide
{
    class ChislExecuter : Executor
    {
        Process process;

        static String FILE_NAME = "log.txt";

        /*Global Parameters*/
        double alphaR = 0;
        double alphaZ = 0;
        double R = 0;
        double l = 0;
        double T = 0;
        double P = 0;
        double a = 0;
        double beta = 0;
        double K = 0;
        double c = 0;
        int I = 0;
        int J = 0;
        int N = 0;
        double hr = 0;
        double hz = 0;
        double ht = 0;

        /*usable variables*/
        double gammaZ = 0;
        double gamma = 0;
        double sigm = 0;

        /*matrix*/
        Double maxT = 0;
        Double minT = Double.MaxValue;

        public Process getProcess(Process processIn)
        {
            process = processIn;
            initParams();
            initFlKoefs();
            process.values = new double[I + 1, J + 1, N + 1];

            executeAlg();

            process.maxTemperature = maxT;
            process.minTemperature = minT;
            return process;

        }

        void initParams()
        {
            alphaR = process.alphaR;
            alphaZ = process.alphaZ;
            process.a = 0.15 * process.R;
            R = process.R;
            l = process.L;
            T = process.T;
            P = process.P;
            a = process.a;
            beta = process.beta;
            K = process.K;
            c = process.c;
            I = process.I;
            J = process.J;
            N = process.N;
            process.hr = R / I;
            process.ht = T / N;
            process.hz = l / J;
            hr = process.hr;
            hz = process.hz;
            ht = process.ht;
        }

        void initFlKoefs()
        {
            gamma = ht * K / (2 * hr * hr);
            gammaZ = ht * K / (2 * hz * hz);
            sigm = 2 * gamma * (1 + (1 + (double)1 / (2 * I)) * hr * alphaR / K);
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

        double[,] getStdMatrix(int rows, int cols)
        {
            return new double[rows, cols];
        }


        double[,] prepareMatrixG()
        {
            double[,] A = getStdMatrix(I + 1, J + 1);
            for (int j = 0; j <= J; j++)
                for (int i = 0; i <= I; i++)
                    A[i, j] = 0.5 * ht * functionG(i, j);
            return A;
        }





        double[,] prepareB(double[,] A1, double[,] A2, int koef)
        {
            double[,] B = getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
                for (int j = 0; j <= J; j++)
                    B[i, j] = koef * (A1[i, j] + A2[i, j]);
            return B;
        }

        double[] proverka(double[,] A, int size, double[] X, double[] B)
        {
            double[] diff = new double[size];
            for (int i = 0; i < size; i++)
            {
                double sum = 0;
                for (int j = 0; j < size; j++)
                {
                    sum += A[i, j] * X[j];
                }
                diff[i] = sum - B[i];
            }
            return diff;
        }


        double[] progonka(double[,] A, double[] B, int size)
        {
            double[] alphaPr = new double[size];
            double[] betaPr = new double[size];
            double[] X = new double[size];
            alphaPr[1] = -(A[0, 1]) / A[0, 0];
            betaPr[1] = B[0] / A[0, 0];

            for (int i = 1; i < size - 1; i++)
            {
                double ei = A[i, i + 1];
                double ci = A[i, i - 1];
                double di = A[i, i];
                double znam = (di + ci * alphaPr[i]);
                alphaPr[i + 1] = (-ei) / znam;
                betaPr[i + 1] = (B[i] - ci * betaPr[i]) / znam;
            }

            double cN = A[size - 1, size - 2];
            double dN = A[size - 1, size - 1];
            double alphaN = alphaPr[size - 1];
            double betaN = betaPr[size - 1];
            double bN = B[size - 1];

            X[size - 1] = (bN - cN * betaN) / (dN + cN * alphaN);

            for (int i = size - 2; i >= 0; i--)
            {
                X[i] = alphaPr[i + 1] * X[i + 1] + betaPr[i + 1];
            }
            MatrixWriter.writeVectorAsString(FILE_NAME, "Proverka ", proverka(A, size, X, B), size, false);
            return X;
        }

        double[] getRow(double[,] A, int i)
        {
            double[] res = new double[J + 1];
            for (int j = 0; j <= J; j++)
                res[j] = A[i, j];
            return res;
        }

        void setRow(double[,] A, double[] Bloc, int i)
        {
            for (int j = 0; j <= J; j++)
                A[i, j] = Bloc[j];
        }

        double[] getCol(double[,] A, int j)
        {
            double[] ret = new double[I + 1];
            for (int i = 0; i <= I; i++)
            {
                ret[i] = A[i, j];
            }
            return ret;
        }

        void setCol(double[,] A, double[] Bloc, int j)
        {
            for (int i = 0; i <= I; i++)
            {
                A[i, j] = Bloc[i];
            }
        }


        double[,] prepareFr()
        {
            double[,] Fr = getStdMatrix(I + 1, I + 1);
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
            double[,] Fl = getStdMatrix(I + 1, J + 1);
            for (int i = 0; i <= I; i++)
            {
                Fl[i, 0] = (c - 2 * gammaZ * (1 + hz * alphaZ / K)) * neededLayer[i, 0] + 2 * gammaZ * (neededLayer[i, 1]);
            }

            for (int i = 0; i <= I; i++)
            {
                Fl[i, J] = (c + 2 * gammaZ * (hz * alphaZ / K - 1)) * neededLayer[i, J] + 2 * gammaZ * (neededLayer[i, J - 1]);
            }

            for (int j = 1; j <= J - 1; j++)
                for (int i = 0; i <= I; i++)
                {
                    Fl[i, j] = gammaZ * neededLayer[i, j - 1] + (c-2 * gammaZ) * neededLayer[i, j] + gammaZ * neededLayer[i, j + 1];
                }
            return Fl;
        }

        double[,] prepareFFr(double[,] neededLayer)
        {
            double[,] Fl = getStdMatrix(I + 1, J + 1);
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
            double[,] Fr = getStdMatrix(J + 1, J + 1);
            Fr[0, 0] = (c + 2 * gammaZ * (1 + hz * alphaZ / K));
            Fr[0, 1] = -2 * gammaZ;
            Fr[J, J - 1] = -2 * gammaZ;
            Fr[J, J] = (c + 2 * gammaZ * (hz * alphaZ / K + 1));

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
                    process.values[i, j, n] = res[i, j];
                    if (res[i, j] > maxT)
                        maxT = res[i, j];
                    if (res[i, j] < minT)
                        minT = res[i, j];
                }

        }

        void executeAlg()
        {
            MatrixWriter.createFile(FILE_NAME);
            double[,] Gsh = prepareMatrixG();
            MatrixWriter.writeMatrixToFile(FILE_NAME, "G", Gsh, I + 1, J + 1, false);
            double[,] tempLayer = getStdMatrix(I + 1, J + 1);
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
                    double[] Bloc = getCol(B, j);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Bloc j=" + j + " n=" + n, Bloc, I + 1, false);
                    double[] Prloc = progonka(Fr, Bloc, I + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Prloc j=" + j + " n=" + n, Prloc, I + 1, false);

                    setCol(tempLayer, Prloc, j);
                    MatrixWriter.writeMatrixToFile(FILE_NAME, "u 1/2 n=" + n, tempLayer, I + 1, J + 1, false);
                }
                Fl = prepareFFr(tempLayer);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "Ffr n=" + n, Fl, I + 1, J + 1, false);
                B = prepareB(Fl, Gsh, 1);
                MatrixWriter.writeMatrixToFile(FILE_NAME, "B2 n=" + n, B, I + 1, J + 1, false);
                for (int i = 0; i <= I; i++)
                {

                    double[] Bloc = getRow(B, i);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Bloc i=" + i + " n=" + n, Bloc, J + 1, true);
                    double[] Prloc = progonka(FFl, Bloc, J + 1);
                    MatrixWriter.writeVectorAsString(FILE_NAME, "Prloc i=" + i + " n=" + n, Prloc, J + 1, true);
                    setRow(tempLayer, Prloc, i);
                    MatrixWriter.writeMatrixToFile(FILE_NAME, "u 1/2 n=" + n, tempLayer, I + 1, J + 1, false);

                }
                MatrixWriter.writeMatrixToFile(FILE_NAME, "u n=" + (n + 1), tempLayer, I + 1, J + 1, false);
                copyToProc(tempLayer, n + 1);
            }
        }

    }
}

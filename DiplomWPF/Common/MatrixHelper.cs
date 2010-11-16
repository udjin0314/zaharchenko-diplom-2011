using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    class MatrixHelper
    {
        public static double[,] getStdMatrix(int rows, int cols)
        {
            return new double[rows, cols];
        }

        public static double[] proverka(double[,] A, int size, double[] X, double[] B)
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

        public static double[] progonka(double[,] A, double[] B, int size)
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
            //MatrixWriter.writeVectorAsString("log.txt", "Proverka ", proverka(A, size, X, B), size, false);
            return X;
        }

        public static double[] getRow(double[,] A, int i, int rows)
        {
            double[] res = new double[rows];
            for (int j = 0; j < rows; j++)
                res[j] = A[i, j];
            return res;
        }

        public static void setRow(double[,] A, double[] Bloc, int i, int rows)
        {
            for (int j = 0; j < rows; j++)
                A[i, j] = Bloc[j];
        }

        public static double[] getCol(double[,] A, int j, int cols)
        {
            double[] ret = new double[cols];
            for (int i = 0; i < cols; i++)
            {
                ret[i] = A[i, j];
            }
            return ret;
        }

        public static void setCol(double[,] A, double[] Bloc, int j, int cols)
        {
            for (int i = 0; i < cols; i++)
            {
                A[i, j] = Bloc[i];
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    class MatrixHelper
    {
        public static float[,] getStdMatrix(int rows, int cols)
        {
            return new float[rows, cols];
        }

        public static float[] proverka(float[,] A, int size, float[] X, float[] B)
        {
            float[] diff = new float[size];
            for (int i = 0; i < size; i++)
            {
                float sum = 0;
                for (int j = 0; j < size; j++)
                {
                    sum += A[i, j] * X[j];
                }
                diff[i] = sum - B[i];
            }
            return diff;
        }

        public static float[] progonka(float[,] A, float[] B, int size)
        {
            float[] alphaPr = new float[size];
            float[] betaPr = new float[size];
            float[] X = new float[size];
            alphaPr[1] = -(A[0, 1]) / A[0, 0];
            betaPr[1] = B[0] / A[0, 0];

            for (int i = 1; i < size - 1; i++)
            {
                float ei = A[i, i + 1];
                float ci = A[i, i - 1];
                float di = A[i, i];
                float znam = (di + ci * alphaPr[i]);
                alphaPr[i + 1] = (-ei) / znam;
                betaPr[i + 1] = (B[i] - ci * betaPr[i]) / znam;
            }

            float cN = A[size - 1, size - 2];
            float dN = A[size - 1, size - 1];
            float alphaN = alphaPr[size - 1];
            float betaN = betaPr[size - 1];
            float bN = B[size - 1];

            X[size - 1] = (bN - cN * betaN) / (dN + cN * alphaN);

            for (int i = size - 2; i >= 0; i--)
            {
                X[i] = alphaPr[i + 1] * X[i + 1] + betaPr[i + 1];
            }
            //MatrixWriter.writeVectorAsString("log.txt", "Proverka ", proverka(A, size, X, B), size, false);
            return X;
        }

        public static float[] getRow(float[,] A, int i, int rows)
        {
            float[] res = new float[rows];
            for (int j = 0; j < rows; j++)
                res[j] = A[i, j];
            return res;
        }

        public static void setRow(float[,] A, float[] Bloc, int i, int rows)
        {
            for (int j = 0; j < rows; j++)
                A[i, j] = Bloc[j];
        }

        public static float[] getCol(float[,] A, int j, int cols)
        {
            float[] ret = new float[cols];
            for (int i = 0; i < cols; i++)
            {
                ret[i] = A[i, j];
            }
            return ret;
        }

        public static void setCol(float[,] A, float[] Bloc, int j, int cols)
        {
            for (int i = 0; i < cols; i++)
            {
                A[i, j] = Bloc[i];
            }
        }

        public static float[] MatrixToVector(float[,] M, ref int maxi, ref int maxj)
        {
            float[] v = new float[maxi * maxj];

            for (int i = 0; i < maxi; i++)
                for (int j = 0; j < maxj; j++)
                    v[i + maxi * j] = M[i, j];

            return v;
        }



        public static float[,] VectorToMatrix(float[] v, ref int maxi, ref int maxj)
        {
            float[,] M = new float[maxi, maxj];

            for (int i = 0; i < maxi; i++)
                for (int j = 0; j < maxj; j++)
                    M[i, j] = v[i + maxi * j];

            return M;
        }
    }
}

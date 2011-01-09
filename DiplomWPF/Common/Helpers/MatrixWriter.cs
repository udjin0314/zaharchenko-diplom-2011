using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    static class MatrixWriter
    {
        public static Boolean logMode = true;

        public static String makeMatrixAsString(String caption, float[,] matrix, int rows, int cols, Boolean transpon)
        {
            String s = "==============================" + caption + "==============================\n";
            if (!transpon)
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                        s += getStringWithNeededLength(matrix[i, j]) + "\t";
                    s += "\n";
                }
            else
            {
                s += "transponated\n";
                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < rows; j++)
                        s += getStringWithNeededLength(matrix[j, i]) + "\t";
                    s += "\n";
                }
            }
            s += "\n";
            return s;

        }

        public static String getStringWithNeededLength(float srcD)
        {
            String src = srcD.ToString();
            int size = 22;
            int strsize = src.Length;
            for (int i = 0; i < size - strsize; i++)
            {
                src += " ";
            }
            return src;
        }

        public static String makeVectorAsString(String caption, float[] vector, int size, Boolean asRow)
        {
            String s = "==============================" + caption + "==============================\n";
            String spacer = "\n";
            if (asRow) spacer = "\t";
            for (int j = 0; j < size; j++)
                s += getStringWithNeededLength(vector[j]) + spacer;
            s += "\n";
            return s;
        }

        public static void createFile(String filename = "log.txt")
        {
            if (logMode) System.IO.File.WriteAllText(filename, "");
        }

        public static void writeMatrixToFile(String caption, float[,] matrix, int rows, int cols, Boolean transpon = false, String filename = "log.txt")
        {
            if (logMode) System.IO.File.AppendAllText(filename, makeMatrixAsString(caption, matrix, rows, cols, transpon));
        }

        public static void writeVectorAsString(String caption, float[] vector, int size, Boolean asRow = false, String filename = "log.txt")
        {
            if (logMode) System.IO.File.AppendAllText(filename, makeVectorAsString(caption, vector, size, asRow));
        }
    }
}

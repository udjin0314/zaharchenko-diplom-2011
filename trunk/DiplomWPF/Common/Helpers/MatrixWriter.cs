using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    static class MatrixWriter
    {
        public static Boolean logMode = false;
        
        public static String makeMatrixAsString(String caption, double[,] matrix, int rows, int cols, Boolean transpon)
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

        public static String getStringWithNeededLength(Double srcD)
        {
            String src = srcD.ToString();
            int size = 22;
            int strsize = src.Length;
            for (int i = 0; i < size -strsize; i++)
            {
                src += " ";
            }
            return src;
        }

        public static String makeVectorAsString(String caption, double[] vector, int size, Boolean asRow)
        {
            String s = "==============================" + caption + "==============================\n";
            String spacer = "\n";
            if (asRow) spacer = "\t";
            for (int j = 0; j < size; j++)
                s += getStringWithNeededLength(vector[j]) + spacer;
            s += "\n";
            return s;
        }

        public static void createFile(String filename)
        {
            if (logMode) System.IO.File.WriteAllText(filename, "");
        }

        public static void writeMatrixToFile(String filename, String caption, double[,] matrix, int rows, int cols, Boolean transpon)
        {
            if (logMode) System.IO.File.AppendAllText(filename, makeMatrixAsString(caption, matrix, rows, cols, transpon));
        }

        public static void writeVectorAsString(String filename, String caption, double[] vector, int size, Boolean asRow)
        {
            if (logMode) System.IO.File.AppendAllText(filename, makeVectorAsString(caption, vector, size, asRow));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    static class MatrixWriter
    {
        public static Boolean logMode = true;

        public static void writeMatrixToFile(String caption, float[,] matrix, int rows, int cols, Boolean transpon = false, String filename = "log.txt")
        {
            System.IO.File.AppendAllText(filename, "==============================" + caption + "==============================\n");
            if (!transpon)
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                        System.IO.File.AppendAllText(filename, getStringWithNeededLength(matrix[i, j]) + "\t");
                    System.IO.File.AppendAllText(filename, "\n");
                }
            else
            {
                System.IO.File.AppendAllText(filename, "transponated\n");
                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < rows; j++)
                        System.IO.File.AppendAllText(filename, getStringWithNeededLength(matrix[j, i]) + "\t");
                    System.IO.File.AppendAllText(filename, "\n");
                }
            }
            System.IO.File.AppendAllText(filename, "\n");

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

        public static void writeVectorAsString(String caption, float[] vector, int size, Boolean asRow = false, String filename = "log.txt")
        {
            System.IO.File.AppendAllText(filename, "==============================" + caption + "==============================\n");
            String spacer = "\n";
            if (asRow) spacer = "\t";
            for (int j = 0; j < size; j++)
                System.IO.File.AppendAllText(filename, getStringWithNeededLength(vector[j]) + spacer);
            System.IO.File.AppendAllText(filename, "\n");
        }

        public static void createFile(String filename = "log.txt")
        {
            if (logMode) System.IO.File.WriteAllText(filename, "");
        }

    }
}

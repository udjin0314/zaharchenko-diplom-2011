using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common.Mathem;
using DiplomWPF.Common.Mathem.Functions;

namespace DiplomWPF.Common.Helpers
{
    class CommonHelper
    {
        public static void readAllLines(String filename, List<Double> targetList)
        {
            if (System.IO.File.Exists(filename))
            {
                String[] lines = System.IO.File.ReadAllLines(filename);
                foreach (String line in lines)
                {
                    targetList.Add(Double.Parse(line));
                }

            }
        }

        public static List<Double> findM(String filename, Int32 size, Function func, double eps, int firstEl = 0)
        {
            int i = 0;
            List<Double> targetList = new List<Double>();
            while (i < firstEl)
            {
                targetList.Add(0);
                i++;
            }
            i = 0;
            double interval = 0.01;
            double first = 0;
            double last = 0;
            Dihotomy mzDihotomy = new Dihotomy(func, eps);
            double value = -200;
            while (i < size)
            {
                double newVal = mzDihotomy.resolve(first, last += interval);
                if (newVal > 0 && value != newVal && newVal > first + eps)
                {
                    value = newVal;
                    targetList.Add(value);
                    i++;
                    first = last;
                }
            }
            String[] lines = new String[size];
            for (int i1 = 0; i1 < size; i1++)
                lines[i1] = targetList[i1].ToString();
            System.IO.File.WriteAllLines(filename, lines);
            return targetList;
        }
    }
}

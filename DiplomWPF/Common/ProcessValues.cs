using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    public class ProcessValues
    {
        private int I;
        private int J;
        private int N;

        private float[][][] values;

        public ProcessValues(int Ii, int Jj, int Nn)
        {
            I = Ii+1;
            J = Jj+1;
            N = Nn+1;
            values = new float[N][][];
            for (int n = 0; n < N; n++)
            {
                values[n] = new float[J][];

                for (int j = 0; j < J; j++)
                {
                    values[n][j] = new float[I];
                }
            }
        }

        public void clear()
        {
            
        }

        public float getPoint(int i, int j, int n)
        {
            if (i > I || j > J || n > N) return 0;
            return values[n][j][i];
        }

        public void setPoint(int i, int j, int n, float u)
        {
            values[n][j][i] = u;
        }

        public float this[int i, int j, int n]
        {
            get { return getPoint(i, j, n); }
            set { setPoint(i, j, n, value); }
        }

    }
}

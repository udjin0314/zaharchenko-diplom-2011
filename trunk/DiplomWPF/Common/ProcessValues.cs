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

        //private List<List<List<float>>> values;
        private float[,,] values;

        public ProcessValues(int Ii, int Jj, int Nn)
        {
            /*I = Ii;
            J = Jj;
            N = Nn;
            values = new List<List<List<float>>>();
            for (int n = 0; n <= N; n++)
            {
                List<List<float>> jsublist = new List<List<float>>();

                for (int j = 0; j <= J; j++)
                {
                    List<float> isublist = new List<float>();

                    for (int i = 0; i <= I; i++)
                    {
                        isublist.Add(0);
                    }
                    jsublist.Add(isublist);
                }
                values.Add(jsublist);
            }*/
            values = new float[Ii, Jj, Nn];
        }

        public float getPoint(int i, int j, int n)
        {
            if (i > I || j > J || n > N) return 0;
            return values[i,j,n];
        }

        public void setPoint(int i, int j, int n, float u)
        {
            values[i, j, n] = u;
        }

        public float this[int i, int j, int n]
        {
            get { return getPoint(i, j, n); }
            set { setPoint(i, j, n, value); }
        }

    }
}

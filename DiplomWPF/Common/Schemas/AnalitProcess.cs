using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DiplomWPF.Common
{

    class AnalitProcess : AbstractProcess
    {
        public static String FILE_NAME = "log.txt";

        private int k = 0;
        private int n = 0;
        private double mkr;
        private double mnz;

        public AnalitProcess(String name, Brush brush)
            : base(name, brush)
        {
        }

        public override void initialize(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T, Int32 N, Int32 I, Int32 J)
        {
            base.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
        }

        public override void initializeParams(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T)
        {
            base.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
        }

        public override void executeProcess()
        {
            //execute();
            base.executeProcess();
            executeAlg();
            isExecuted = true;
            
        }

        public override void executeProcess(object parameters)
        {
            //execute();
            base.executeProcess(parameters);
            executeAlg();
            isExecuted = true;

        }

        private double resolveMkr(double r)
        {

        }

        public void executeAlg()
        {
            for (int n = 0; n <= N - 1; n++)
            {
                for (int i = 0; i <= I; i++)
                    for (int j = 0; j <= J; j++)
                    {
                        values[i, j, n]=;
                    }
            }
        }

    }
}

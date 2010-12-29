__kernel void firstrun(__global int* IJ,
						__global       float * F,
                      __global       float * B,
                      __global       float * tempLayer,
						__local       float * alphaPr,
						__local       float * betaPr)
{
	int I = IJ[0];
                int J = IJ[1];
                int jG = get_global_id(0);

                int size = I + 1;

                alphaPr[1] = -(F[size]) / F[0];
                betaPr[1] = B[size * jG] / F[0];
                for (int i = 1; i < size - 1; i++)
                {
                    float ei = F[i + size * (i + 1)];
                    float ci = F[i + size * (i - 1)];
                    float di = F[i + size * (i)];
                    float znam = (di + ci * alphaPr[i]);
                    alphaPr[i + 1] = (-ei) / znam;
                    betaPr[i + 1] = (B[i + size * jG] - ci * betaPr[i]) / znam;
                }

                float cN = F[size - 1 + size * (size - 2)];
                float dN = F[size - 1 + size * (size - 1)];
                float alphaN = alphaPr[size - 1];
                float betaN = betaPr[size - 1];
                float bN = B[size - 1 + size * jG];
                tempLayer[size - 1 + size * jG] = (bN - cN * betaN) / (dN + cN * alphaN);
                for (int i = size - 2; i >= 0; i--)
                {
                    tempLayer[i + size * jG] = alphaPr[i + 1] * tempLayer[i + 1 + size * jG] + betaPr[i + 1];
                }
}



__kernel void secondrun(__global int* IJ,
						__global       float * F,
                      __global       float * B,
                      __global       float * tempLayer,
						__local       float * alphaPr,
						__local       float * betaPr)
{
	int I=IJ[0];
	int J=IJ[1];
	int iG = get_global_id(0);


	int cols = I+1;
	int size = J+1;

	alphaPr[1] = -(F[size]) / F[0];
	betaPr[1] = B[iG] / F[0];
	for (int i = 1; i < size - 1; i++)
    {
		float ei = F[i+size*(i + 1)];
        float ci = F[i+size*(i - 1)];
        float di = F[i+size*(i)];
        float znam = (di + ci * alphaPr[i]);
        alphaPr[i + 1] = (-ei) / znam;
        betaPr[i + 1] = (B[iG+cols*i] - ci * betaPr[i]) / znam;
    }

    float cN = F[size - 1+size*(size - 2)];
    float dN = F[size - 1+size*(size - 1)];
    float alphaN = alphaPr[size - 1];
    float betaN = betaPr[size - 1];
    float bN = B[iG+cols*(size - 1)];
	tempLayer[iG+cols*(size - 1)] = (bN - cN * betaN) / (dN + cN * alphaN);
	for (int i = size - 2; i >= 0; i--)
    {
		tempLayer[iG+cols*i] = alphaPr[i + 1] * tempLayer[iG+cols*(i + 1)] + betaPr[i + 1];
    }
}
#define M_PI (3.14159265358979323846f)

__kernel void firstrun(__global int* IJ,
						__global       float * a,
						__global       float * b,
						__global       float * c,
                      __global       float * B,
                      __global       float * tempLayer)
{
	int I = IJ[0];
    int jG = get_global_id(0);

    int n = I + 1;

    for (int i = 1; i < n; i++)
    {
        float m = b[i - 1] / a[i - 1];
        a[i] = a[i] - m * c[i - 1];
        B[i + n * jG] = B[i + n * jG] - m * B[i - 1 + n * jG];
    }

    tempLayer[n - 1 + n * jG] = B[n - 1 + n * jG] / a[n - 1];

    for (int i = n - 2; i >= 0; i--)
       tempLayer[i + n * jG] = (B[i + n * jG] - c[i] * tempLayer[i + 1 + n * jG]) / a[i];
}

__kernel void secondrun(__global int* IJ,
						__global       float * a,
						__global       float * b,
						__global       float * c,
                      __global       float * B,
                      __global       float * tempLayer)
{
	int I=IJ[0];
	int J=IJ[1];
	int iG = get_global_id(0);


	int cols = I + 1;
    int n = J + 1;

    for (int i = 1; i < n; i++)
   {
      float m = b[i - 1] / a[i - 1];
      a[i] = a[i] - m * c[i - 1];
       B[iG + cols * i] = B[iG + cols * i] - m * B[iG + cols * (i - 1)];
    }

    tempLayer[iG + cols * (n - 1)] = B[iG + cols * (n - 1)] / a[n - 1];

   for (int i = n - 2; i >= 0; i--)
       tempLayer[iG + cols * i] = (B[iG + cols * i] - c[i] * tempLayer[iG + cols * (i + 1)]) / a[i];
}



__kernel void prepareMatrixG(__global int* IJ, __global float * params, __global  float * A)
{
	int iG = get_global_id(0);
	int jG = get_global_id(1);
	int I=IJ[0];
	int J=IJ[1];
	float hr=params[0];
	float hz=params[1];
	float ht=params[2];
	float a=params[3];
	float P=params[4];
	float beta=params[5];
	int cols = I + 1;
	A[iG+cols*jG] = 0.5f * ht * P * beta / (M_PI * a * a) * exp(-(beta * jG*hz + (iG*iG*hr*hr / (a * a))));
}

__kernel void prepareFr(__global int* IJ, __global float * params,	__global  float * A, __global  float * B, __global  float * C)
{
	int iG = get_global_id(0);
	int I=IJ[0]+1;
	float gamma=params[0];
	float sigm=params[1];
	float c=params[2];
	if (iG==0) C[iG] = 4 * gamma;
	if (iG>0 && iG<=I-2) C[iG] = gamma * (1.0f + 1.0f / (2.0f * iG));
	if (iG==I-1) A[iG-I+1]=-(4 * gamma + c);
	if (iG>=I && iG<=2*I-3) A[iG-I+1]=-(2 * gamma + c);
	if (iG==2*I-2) A[iG-I+1] =-(sigm + c);
	if (iG>=2*I-1&&iG<=3*I-4) B[iG-2*I+1]=gamma * (1.0f - 1.0f / (2.0f * (iG-2*I+2)));
	if (iG==3*I-3) B[iG-2*I+1]=2 * gamma; 
}

__kernel void prepareFFl(__global int* IJ, __global float * params, __global  float * A, __global  float * B, __global  float * C)
{
	int iG = get_global_id(0);
	int J=IJ[1]+1;
	float gammaZ=params[0];
	float sigmZ=params[1];
	float c=params[2];
	if (iG==0) C[iG] = -2 * gammaZ;
	if (iG>0 && iG<=J-2) C[iG] = -gammaZ;
	if (iG==J-1) A[iG-J+1]=(c + sigmZ);
	if (iG>=J && iG<=2*J-3) A[iG-J+1]=(2 * gammaZ + c);
	if (iG==2*J-2) A[iG-J+1] =(c + sigmZ);
	if (iG>=2*J-1&&iG<=3*J-4) B[iG-2*J+1]=-gammaZ;
	if (iG==3*J-3) B[iG-2*J+1]=-2 * gammaZ; 
}

__kernel void prepareFl(__global int* IJ, __global float * params, __global  float * neededLayer, __global  float * Fl)
{
	int iG = get_global_id(0);
	int jG = get_global_id(1);
	int I=IJ[0];
	int J=IJ[1];
	int cols = I + 1;
	float gammaZ=params[0];
	float sigmZ=params[1];
	float c=params[2];
	
	if (jG==0) Fl[iG]=(c - sigmZ) * neededLayer[iG] + 2 * gammaZ * (neededLayer[iG+cols]);
	if (jG==J) Fl[iG+cols*J] = (c - sigmZ) * neededLayer[iG+cols*J] + 2 * gammaZ * (neededLayer[iG+cols*(J - 1)]);
	if (jG>0&&jG<J) Fl[iG+cols*jG] = gammaZ * neededLayer[iG+cols*(jG-1)] + (c - 2 * gammaZ) * neededLayer[iG+cols*jG] + gammaZ * neededLayer[iG+cols*(jG+1)];
}

__kernel void prepareFFr(__global int* IJ, __global float * params, __global  float * neededLayer, __global  float * Fl)
{
	int iG = get_global_id(0);
	int jG = get_global_id(1);
	int I=IJ[0];
	int J=IJ[1];
	int cols = I + 1;
	float gamma=params[0];
	float sigm=params[1];
	float c=params[2];
	if(iG==0) Fl[cols*jG] = (c - 4 * gamma) * (neededLayer[cols*jG]) + 4 * gamma * (neededLayer[1+cols*jG]);	
	if(iG==I) Fl[I+cols*jG] = (2 * gamma) * (neededLayer[I - 1+cols*jG]) - (sigm - c) * (neededLayer[I+cols*jG]);
	if (iG>0&&iG<I) Fl[iG+cols*jG] = gamma * (1 - 1.0f / (2 * iG)) * neededLayer[iG - 1+cols*jG] - (2 * gamma - c) * neededLayer[iG+cols*jG] + gamma * (1 + 1.0f / (2 * iG)) * neededLayer[iG + 1+cols*jG];
}

__kernel void prepareBFirst(__global int* IJ, __global float * Fl, __global  float * Gsh, __global  float * B)
{
	int iG = get_global_id(0);
	int jG = get_global_id(1);
	int I=IJ[0];
	int J=IJ[1];
	int cols = I + 1;
	B[iG+cols*jG] = -1 * (Fl[iG+cols*jG] + Gsh[iG+cols*jG]);
}

__kernel void prepareBSecond(__global int* IJ, __global float * Fl, __global  float * Gsh, __global  float * B)
{
	int iG = get_global_id(0);
	int jG = get_global_id(1);
	int I=IJ[0];
	int J=IJ[1];
	int cols = I + 1;
	B[iG+cols*jG] = (Fl[iG+cols*jG] + Gsh[iG+cols*jG]);
}
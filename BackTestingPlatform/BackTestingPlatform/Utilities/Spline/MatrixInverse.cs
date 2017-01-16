using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace BackTestingPlatform.Utilities.Spline
{
    public static class MatrixInverse
    {
        public static Vector<double> getInverse(double[,] A, double[] b)
        {
            var matrixA = DenseMatrix.OfArray(A);
            var vectorB = DenseVector.OfArray(b);
            var resultX = matrixA.LU().Solve(vectorB);
            return resultX;
        }
    }
}

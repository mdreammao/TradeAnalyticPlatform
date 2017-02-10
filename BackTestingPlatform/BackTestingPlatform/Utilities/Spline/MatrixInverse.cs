using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
//我曾做过测试，将double[,] 先转成Math矩阵，然后进行矩阵运算，再利用matrix2.ToArray()将Math矩阵转换成double[,]，其运算时间和直接利用C#编写的矩阵运算相差很小。
// 但如果是利用for循环将double数组的数值赋值给Math矩阵进行矩阵运算，然后再利用for循环将Math矩阵赋值给某个double[，] 数组，其运算时间可以减少1/3。在开发效率和运算效率上，使用的时候可以根据需要进行取舍。
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

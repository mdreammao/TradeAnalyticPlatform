using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics;

namespace BackTestingPlatform.Utilities.Spline
{
    public class CubicSpline
    {
        public static IInterpolation nuaturalSpline(double[] x,double[] y)
        {
            return Interpolate.CubicSplineRobust(x, y);
        }
    }
}

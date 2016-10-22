using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.LogicFunction
{
    public class Cross
    {
        public static bool crossUp(List<double> series1, List<double> series2, int index)
        {
            bool isCrossUp = false;
            if (index != 0)
            {
                if (series1[index - 1] < series2[index - 1] && series1[index] > series2[index])
                    isCrossUp = true;
            }
            return isCrossUp;
        }

        public static bool crossDown(List<double> series1, List<double> series2, int index)
        {
            bool isCrossDown = false;
            if (index != 0)
            {
                if (series1[index - 1] > series2[index - 1] && series1[index] < series2[index])
                    isCrossDown = true;
            }
            return isCrossDown;
        }

    }
}

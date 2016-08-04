using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    /// <summary>
    /// 股票盘口价格的格式，不定长度的N挡盘口价量
    /// </summary>
    public class PositionData
    {
        public double price, volume;
        public PositionData(double price, double volume)
        {
            this.price = price;
            this.volume = volume;
        }
    }
}

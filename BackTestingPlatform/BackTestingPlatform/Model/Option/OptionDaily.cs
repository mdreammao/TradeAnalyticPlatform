using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
    /// <summary>
    /// 期权 基础信息
    /// </summary>

    public interface IOptionInfo
    {
        string optionCode { get; set; }
        string optionName { get; set; }
        string executeType { get; set; }
        double strike { get; set; }
        string optionType { get; set; }
        DateTime startDate { get; set; }
        DateTime endDate { get; set; }
    }
    public class OptionInfo : IOptionInfo
    {
        public string optionCode { get; set; }
        public string optionName { get; set; }
        public string executeType { get; set; }
        public double strike { get; set; }
        public string optionType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }




    public class OptionDailyWithInfo : OptionDaily, IOptionInfo
    {
        public string optionCode { get; set; }
        public string optionName { get; set; }
        public string executeType { get; set; }
        public double strike { get; set; }
        public string optionType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }


    public class OptionDaily : KLine
    {
    }

}

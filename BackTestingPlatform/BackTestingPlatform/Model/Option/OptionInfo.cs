using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
    public class OptionInfo
    {
        public string optionCode { get; set; }
        public string optionName;
        public string executeType;
        public double strike;
        public string optionType;
        public DateTime startDate, endDate;
    }


}

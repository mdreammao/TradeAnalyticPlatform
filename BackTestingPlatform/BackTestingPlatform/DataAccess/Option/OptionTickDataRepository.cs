using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Model;


namespace BackTestingPlatform.DataAccess.Option
{

    public interface OptionTickDataRepository
    {
         List<OptionTickData> fetchDataDaily(string optionCode, string date);
    }

    public class OptionTickDataRepositoryFromMSSQL : OptionTickDataRepository
    {
        public List<OptionTickData> fetchDataDaily(string optionCode, string date)
        {
            List<OptionTickData> items = new List<OptionTickData>();

            return items;
        }

    }

}

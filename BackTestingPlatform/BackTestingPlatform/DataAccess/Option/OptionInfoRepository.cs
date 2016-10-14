using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Common;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Option
{
    public class OptionInfoRepository : BasicDataRepository<OptionInfo>
    {

        public List<OptionInfo> readFromWind(string underlying="510050.SH", string market="sse")
        {
            string marketStr = "";
            if (market == "sse")
            {
                marketStr = ".SH";
            }
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.wset("optioncontractbasicinfo", "exchange=" + market + ";windcode=" + underlying + ";status=all");
            int len = wd.codeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<OptionInfo> items = new List<OptionInfo>(len);
            object[] dm = (object[])wd.data;
            for (int k = 0; k < len; k++)
            {
                items.Add(new OptionInfo
                {
                    optionCode = (string)dm[k * fieldLen + 0] + marketStr,
                    optionName = (string)dm[k * fieldLen + 1],
                    executeType = (string)dm[k * fieldLen + 5],
                    strike = (double)dm[k * fieldLen + 6],
                    optionType = (string)dm[k * fieldLen + 4],
                    startDate = (DateTime)dm[k * fieldLen + 9],
                    endDate = (DateTime)dm[k * fieldLen + 10]
                });
            }
            return items;
        }

        protected override List<OptionInfo> readFromWind()
        {
            return readFromWind("510050.SH", "sse");
        }
    }
}

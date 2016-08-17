using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;
using System.IO;
using System.Linq;
using BackTestingPlatform.Utilities;
using System.Configuration;
using System.Data;
using System.Globalization;
using BackTestingPlatform.DataAccess.Common;

namespace BackTestingPlatform.DataAccess.Option
{

    public class OptionDailyRepository : BasicDataRepository<OptionDaily>
    {
        
        
        

        public List<OptionDaily> readFromWind(string code, string market)
        {            
            string marketStr = "";
            if (market == "sse")
            {
                marketStr = ".SH";
            }
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.wset("optioncontractbasicinfo", "exchange=" + market + ";windcode=" + code + ";status=all");
            int len = wd.codeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<OptionDaily> items = new List<OptionDaily>(len * fieldLen);
            object[] dm = (object[])wd.data;
            for (int k = 0; k < len; k++)
            {
                items.Add(new OptionDaily
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

        protected override List<OptionDaily> readFromWind()
        {
            return readFromWind("510050.SH", "sse");
        }
    }


}

using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BackTestingPlatform.Core;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Stock
{
    public class TickRepository : SequentialDataRepository<Tick>
    {
        public override List<Tick> fetchFromDefaultMssql(string code, string date)
        {
            throw new NotImplementedException();
        }

        public override List<Tick> fetchFromWind(string code, string date)
        {
            WindAPI wapi = Platforms.GetWindAPI();

            WindData d = wapi.wsq(code, "rt_last_vol,rt_latest,rt_vol,rt_amt,rt_chg,rt_pct_chg,rt_high_limit,rt_low_limit,rt_swing,rt_vol_ratio,rt_turn,rt_mkt_cap,rt_oi_chg,rt_pre_close_dp,rt_ask1,rt_ask2,rt_bid1,rt_bid2,rt_bsize1,rt_bsize2,rt_delta,rt_imp_volatility", "");

            //todo wsq只返回一行
            var xxx = d.data;
            return null;
        }

        public override Tick toEntity(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}

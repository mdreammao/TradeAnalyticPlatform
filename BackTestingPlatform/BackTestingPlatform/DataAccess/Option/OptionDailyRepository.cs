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
        protected override List<OptionDaily> readFromWind()
        {
            throw new NotImplementedException();
        }
    }


}

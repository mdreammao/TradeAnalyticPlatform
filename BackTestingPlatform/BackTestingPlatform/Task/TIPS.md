=== 常用CODING TIPS ===

* 获取appSetting里的配置
string value = System.Configuration.ConfigurationManager.AppSettings["myKey"];

* 字符串转换为日期
DateTime dt=DateTime.ParseExact(str, "yyyyMMddhhmmss", CultureInfo.InvariantCulture);

* 获取Platforms.basicInfo里面的全局变量，例如TradeDays
List<DateTime> tradeDays=(List<DateTime>)Platforms.basicInfo["TradeDays"];
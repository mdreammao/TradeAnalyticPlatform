# 常用代码提示 USUAL CODING TIPS 

## 获取appSetting里的配置
```
string value = System.Configuration.ConfigurationManager.AppSettings["myKey"];
```

## 字符串,数值，日期之间的转换
```
//使用原生api,字符串转换为日期
DateTime dt=DateTime.ParseExact(str, "yyyyMMddhhmmss", CultureInfo.InvariantCulture);

//使用自定义函数，请参见Utilities下的Kit.cs,例如:
DateTime dt=Kit.toDateTime("20160805");	//字符串转换为日期
DateTime dt=Kit.toDateTime("20160805134545");	//字符串转换为日期
DateTime dt=Kit.toDateTime(tdate,time);
int x=Kit.to14digitsInt(dt);
```

## 获取Platforms.basicInfo里面的全局变量，例如TradeDays
```
List<DateTime> tradeDays=(List<DateTime>)Platforms.basicInfo["TradeDays"];
```

## 获取Service,Repository等实例（全局共享的唯一实例，由autofac托管）。
```
    TradeDaysService tradeDaysService = Platforms.container.Resolve<TradeDaysService>();
    TradeDaysRepository tradeDaysRepository = Platforms.container.Resolve<TradeDaysRepository>();
```



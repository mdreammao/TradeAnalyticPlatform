using System;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using NLog;
using BackTestingPlatform.Utilities;
using System.Collections.Generic;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.DataAccess.Stock;
using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.Utilities.Common;

namespace BackTestingPlatform.Charts
{
   /// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class CandleStick : Form
	{
		private ZedGraphControl zedG;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;
        static Logger log = LogManager.GetCurrentClassLogger();

        //Form 的全局变量
        private DateTime startTime, endTime;
        private string secCode = "";
        private int frequency;

        /// <param name="startTime"></param>K线图的起始时间
        /// <param name="endTime"></param>K线图的结束时间
        /// <param name="secCode"></param>K线图标的名称
        /// <param name="frequency"></param>请求标的的显示频率，
        /// 0 tick, 1 1min, 2 5min， 15min，3 30min, 4 60min, 5 1day，
        public CandleStick(int startTime, int endTime, string secCode, int frequency)
		{
			//初始化显示图片的基本格式
			InitializeComponent();
            this.startTime = Kit.ToDate(startTime);
            this.endTime = Kit.ToDate(endTime);
            this.secCode = secCode;
            this.frequency = frequency;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            //显示的属性设置，后期还要做美工处理**************
            //获得屏幕大小
            int iActulaWidth = Screen.PrimaryScreen.Bounds.Width;
            int iActulaHeight = Screen.PrimaryScreen.Bounds.Height;

            zedG = new ZedGraphControl();
            SuspendLayout();
            // 图片属性设置 
            zedG.IsShowPointValues = false;
            zedG.Location = new Point(0, 0);
            zedG.Name = "z1";
            zedG.PointValueFormat = "G";
            zedG.Size = new Size(iActulaWidth+iActulaWidth/2, iActulaHeight+iActulaHeight/2);
            zedG.TabIndex = 0;
            // Form属性设置
            AutoScaleBaseSize = new Size(10, 24);
            ClientSize = new Size(iActulaWidth+iActulaWidth/2, iActulaHeight+iActulaHeight/2);
            Controls.Add(zedG);
            Name = "Form1";
            Text = "Form1";
            Load += new EventHandler(Form_Load);
            ResumeLayout(false);

            //设置左右拖拽功能
            zedG.PanModifierKeys = Keys.None;

            //同步缩放，还是不成功，会影响坐标轴
            zedG.IsSynchronizeXAxes = true;
            zedG.IsSynchronizeYAxes = false;

            //使用键盘聚焦还没有成功
            zedG.ZoomModifierKeys = Keys.Down;

        }
        #endregion

        #region 在K线图中加载Sec数据的设置
        /// <summary>
        /// 取出1分钟数据的时间，开收盘价，高低价，成交量等信息输入该图
        /// </summary>
        private void Form_Load( object sender, EventArgs e )
		{
            //画一张大图，包含价格K线和成交量
            MasterPane myPaneMaster = zedG.MasterPane;
            myPaneMaster.Title.Text = secCode;
            myPaneMaster.Title.FontSpec.FontColor = Color.Black;

            //PaneMaster里面画一张价格的小图
            GraphPane panePrice = new GraphPane(new Rectangle(10, 10, 10, 10), "Mes", " t ( h )", "Rate");
            myPaneMaster.PaneList[0] = (panePrice);
            //PaneMaster里面画一张成交量的小图
            GraphPane paneVolume = new GraphPane(new Rectangle(10, 10, 10, 10), "Mes", " t ( h )", "Rate");
            myPaneMaster.PaneList.Add(paneVolume);

            //蜡烛线例子
            //设置名称和坐标轴
            panePrice.Title.Text = "K线图";
            panePrice.XAxis.Title.Text = "日期";
            panePrice.XAxis.Title.FontSpec.FontColor = Color.Black;
            panePrice.YAxis.Title.Text = "价格";
            panePrice.YAxis.Title.FontSpec.FontColor = Color.Black;

            //spl装载时间，价格数据
            StockPointList spl = new StockPointList();
            Random rand = new Random();

            //将系统时间转化为xDate时间
            XDate xStart = XDate.DateTimeToXLDate(startTime);
            XDate xEnd = XDate.DateTimeToXLDate(endTime);

            //取Sec的分钟数据，存储于data中
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startTime, endTime);

            //数据准备，取minute数据，然后再将数据进行转换为各个频率
            Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
            foreach (var tempDay in tradeDays)
            {
                var stockData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(secCode, tempDay);
                if (!data.ContainsKey(secCode))
                    data.Add(secCode, stockData.Cast<KLine>().ToList());
                else
                    data[secCode].AddRange(stockData.Cast<KLine>().ToList());
            }

            //定义变量存储分钟数据
            Dictionary<string, List<KLine>> minuteData = new Dictionary<string, List<KLine>>();
            foreach (var variety in data)
            {
                minuteData.Add(variety.Key, data[variety.Key]);
            }

            //定义成交量
            double[] volume = new double[minuteData[secCode].Count];
            //根据频率选择累加的时间
            switch (frequency)
            {
                //取tick数据
                case 0:
                    log.Info("暂时没有tick数据");
                    break;

                //1min K线
                case 1:
                    for (int i = 0; i < minuteData[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteData[secCode][i].open;
                        double close = minuteData[secCode][i].close;
                        double high = minuteData[secCode][i].high;
                        double low = minuteData[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加1分钟
                        xStart.AddMinutes(1.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;

                //显示5min K线
                case 2:
                    Dictionary<string, List<KLine>> minuteData5Min = new Dictionary<string, List<KLine>>();
                    foreach (var variety in data)
                    {
                        List<KLine> data5K = new List<KLine>();
                        data5K = MinuteFrequencyTransferUtils.MinuteToNPeriods(minuteData[variety.Key], "Minutely", 5);
                        minuteData5Min.Add(variety.Key, data5K);
                    }
                    for (int i = 0; i < minuteData5Min[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteData5Min[secCode][i].open;
                        double close = minuteData5Min[secCode][i].close;
                        double high = minuteData5Min[secCode][i].high;
                        double low = minuteData5Min[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加5分钟
                        xStart.AddMinutes(5.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;

                //显示15min K线
                case 3:
                    Dictionary<string, List<KLine>> minuteData15Min = new Dictionary<string, List<KLine>>();
                    foreach (var variety in data)
                    {
                        List<KLine> data15K = new List<KLine>();
                        data15K = MinuteFrequencyTransferUtils.MinuteToNPeriods(minuteData[variety.Key], "Minutely", 15);
                        minuteData15Min.Add(variety.Key, data15K);
                    }
                    for (int i = 0; i < minuteData15Min[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteData15Min[secCode][i].open;
                        double close = minuteData15Min[secCode][i].close;
                        double high = minuteData15Min[secCode][i].high;
                        double low = minuteData15Min[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加15分钟
                        xStart.AddMinutes(15.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;

                //显示30min K线
                case 4:
                    Dictionary<string, List<KLine>> minuteData30Min = new Dictionary<string, List<KLine>>();
                    foreach (var variety in data)
                    {
                        List<KLine> data30K = new List<KLine>();
                        data30K = MinuteFrequencyTransferUtils.MinuteToNPeriods(minuteData[variety.Key], "Minutely", 30);
                        minuteData30Min.Add(variety.Key, data30K);
                    }
                    for (int i = 0; i < minuteData30Min[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteData30Min[secCode][i].open;
                        double close = minuteData30Min[secCode][i].close;
                        double high = minuteData30Min[secCode][i].high;
                        double low = minuteData30Min[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加30分钟
                        xStart.AddMinutes(30.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;

                //显示60min K线
                case 5:
                    Dictionary<string, List<KLine>> minuteData60Min = new Dictionary<string, List<KLine>>();
                    foreach (var variety in data)
                    {
                        List<KLine> data60K = new List<KLine>();
                        data60K = MinuteFrequencyTransferUtils.MinuteToNPeriods(minuteData[variety.Key], "Minutely", 60);
                        minuteData60Min.Add(variety.Key, data60K);
                    }
                    for (int i = 0; i < minuteData60Min[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteData60Min[secCode][i].open;
                        double close = minuteData60Min[secCode][i].close;
                        double high = minuteData60Min[secCode][i].high;
                        double low = minuteData60Min[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加60分钟
                        xStart.AddMinutes(60.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;

                //显示日K线
                case 6:
                    Dictionary<string, List<KLine>> minuteDataDaily = new Dictionary<string, List<KLine>>();
                    foreach (var variety in data)
                    {
                        List<KLine> dataDaily = new List<KLine>();
                        dataDaily = MinuteFrequencyTransferUtils.MinuteToNPeriods(minuteData[variety.Key], "Minutely", 240);
                        minuteDataDaily.Add(variety.Key, dataDaily);
                    }
                    for (int i = 0; i < minuteDataDaily[secCode].Count; i++)
                    {
                        double timePoint = i;
                        double open = minuteDataDaily[secCode][i].open;
                        double close = minuteDataDaily[secCode][i].close;
                        double high = minuteDataDaily[secCode][i].high;
                        double low = minuteDataDaily[secCode][i].low;
                        volume[i] = minuteData[secCode][i].volume;

                        StockPt pt = new StockPt(timePoint, high, low, open, close, volume[i]);
                        spl.Add(pt);

                        // 时间加1天
                        xStart.AddDays(1.0);
                        // but skip the weekends
                        if (XDate.XLDateToDayOfWeek(xStart.XLDate) == 6)
                            xStart.AddDays(2.0);
                    }
                    break;
            }

            //添加栅格线
            //myPane.XAxis.MajorGrid.IsVisible = true;
            //myPane.YAxis.MajorGrid.IsVisible = true;
            //myPane.XAxis.MajorGrid.Color = Color.LightGray;
            //myPane.YAxis.MajorGrid.Color = Color.LightGray;
            //myPane.YAxis.MajorGrid.DashOff = 0;
            //myPane.XAxis.MajorGrid.DashOff = 0;


            panePrice.XAxis.Type = AxisType.Date;
            panePrice.XAxis.Scale.Format = "MM-dd";
            //myPane.XAxis.Scale.FontSpec.Angle = 45;//X轴文字方向，0-90度
            //开始Y轴坐标设置
            ////设置Y轴坐标的范围
            //myPane.YAxis.Scale.Max = Math.Round(maxhi * 1.2, 2);//Math.Ceiling(maxhi);
            //myPane.YAxis.Scale.Min = Math.Round(minlow * 0.8, 2);

            //Y轴最大刻度，注意minStep只会显示刻度线不会显示刻度值，minStep为纵坐标步长
            panePrice.YAxis.Scale.MajorStep = 0.01;

            //myPane.XAxis.Scale.FontSpec.FontColor = Color.Black;
            //myPane.YAxis.Scale.FontSpec.FontColor = Color.Black;

            panePrice.XAxis.Type = AxisType.DateAsOrdinal;
            //myPane.Legend.FontSpec.Size = 18f;
            //myPane.Legend.Position = LegendPos.InsideTopRight;
            //myPane.Legend.Location = new Location(0.5f, 0.6f, CoordType.PaneFraction,
            //    AlignH.Right, AlignV.Top);
            JapaneseCandleStickItem myCurve = panePrice.AddJapaneseCandleStick(secCode, spl);
            myCurve.Stick.IsAutoSize = true;
            //myCurve.Stick.Color = Color.Blue;
            myCurve.Stick.FallingFill = new Fill(Color.Green);//下跌颜色
            myCurve.Stick.RisingFill = new Fill(Color.Red);//上扬颜色

            // pretty it up a little
            //myPane.Chart.Fill = new Fill(Color.LightBlue, Color.LightGoldenrodYellow, 135.0f);
            //myPane.Fill = new Fill(Color.Orange, Color.FromArgb(220, 220, 255), 45.0f);
            Color c1 = ColorTranslator.FromHtml("#ffffff");
            Color c2 = ColorTranslator.FromHtml("#ffd693");
            panePrice.Chart.Fill = new Fill(c1);//图形区域颜色
            panePrice.Fill = new Fill(c2);//整体颜色 


            //成交量线例子
            // Set the Titles
            paneVolume.Title.Text = "成交量";
            paneVolume.XAxis.Title.Text = "Time";
            paneVolume.YAxis.Title.Text = "Volume Num";

            // Make up some random data points
            //string[] labels = { "Panther", "Lion", "Cheetah","Cougar", "Tiger", "Leopard" };

            //double[] y1 = { 100, 115, 75, 22, 98, 40, -100, -20 };
            //double[] y2 = { 90, 100, 95, 35, 80, 35 };
            //double[] y3 = { 80, 110, 65, 15, 54, 67 };
            //double[] y4 = { 120, 125, 100, 40, 105, 75 };

            // Generate a red bar with "Curve 1" in the legend
            BarItem myBar = paneVolume.AddBar(null, null, volume, Color.Red);
            //myBar.Bar.Fill = new Fill(Color.Red);

            // Generate a blue bar with "Curve 2" in the legend
            //myBar = paneVolume.AddBar("Curve 2", null, y2, Color.Blue);
            //myBar.Bar.Fill = new Fill(Color.Blue, Color.White, Color.Blue);
            //设置bar宽度
            paneVolume.BarSettings.ClusterScaleWidth = 0.5;
            log.Info(paneVolume.BarSettings.GetClusterWidth());
            paneVolume.BarSettings.Type = BarType.Cluster;

            // Generate a green bar with "Curve 3" in the legend
            //myBar = myPane.AddBar("Curve 3", null, y3, Color.Green);
            //myBar.Bar.Fill = new Fill(Color.Green, Color.White,
            // Color.Green);

            // Generate a black line with "Curve 4" in the legend
            //LineItem myCurve = myPane.AddCurve("Curve 4",
            //null, y4, Color.Black, SymbolType.Circle);
            //myCurve.Line.Fill = new Fill(Color.White,
            //Color.LightSkyBlue, -45F);

            // Fix up the curve attributes a little
            //myCurve.Symbol.Size = 8.0F;
            //myCurve.Symbol.Fill = new Fill(Color.White);
            //myCurve.Line.Width = 2.0F;

            // Draw the X tics between the labels instead of 
            // at the labels
            paneVolume.XAxis.MajorTic.IsBetweenLabels = true;

            // Set the XAxis labels
            //myPane.XAxis.Scale.TextLabels = labels;
            // Set the XAxis to Text type
            paneVolume.XAxis.Type = AxisType.Text;

            // Fill the Axis and Pane backgrounds
            paneVolume.Chart.Fill = new Fill(Color.White,
                  Color.FromArgb(255, 255, 166), 90F);
            paneVolume.Fill = new Fill(Color.FromArgb(250, 250, 255));

            using (Graphics g = CreateGraphics())
                myPaneMaster.SetLayout(g, 2, 0);
            zedG.AxisChange();
        }
        #endregion
    }
}

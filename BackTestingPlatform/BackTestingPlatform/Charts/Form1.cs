using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using NLog;

namespace BackTestingPlatform.Charts
{
   /// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private ZedGraph.ZedGraphControl z1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        static Logger log = LogManager.GetCurrentClassLogger();

        public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            z1 = new ZedGraphControl();
            SuspendLayout();
            // 
            // z1
            // 
            z1.IsShowPointValues = false;
            z1.Location = new Point(0, 0);
            z1.Name = "z1";
            z1.PointValueFormat = "G";
            z1.Size = new Size(1360, 764);
            z1.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleBaseSize = new Size(10, 24);
            ClientSize = new Size(923, 538);
            Controls.Add(z1);
            Name = "Form1";
            Text = "Form1";
            Load += new EventHandler(Form1_Load);
            ResumeLayout(false);

        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        /*
        static void Main() 
		{
			Application.Run( new Form1() );
		}
        */

		private void Form1_Load( object sender, EventArgs e )
		{
            GraphPane myPane = z1.GraphPane;

            //蜡烛线例子
            // Set the title and axis labels   
            myPane.Title.Text = "K线图";
            myPane.XAxis.Title.Text = "日期";
            myPane.XAxis.Title.FontSpec.FontColor = Color.Black;
            myPane.YAxis.Title.Text = "价格";
            myPane.YAxis.Title.FontSpec.FontColor = Color.Black;

            //Get Data
            StockPointList spl = new StockPointList();
            Random rand = new Random();

            // First day is jan 1st
            XDate xDate = new XDate(2006, 1, 1);
            double open = 50.0;

            for (int i = 0; i < 50; i++)
            {
                double x = xDate.XLDate;
                double close = open + rand.NextDouble() * 10.0 - 5.0;
                double hi = Math.Max(open, close) + rand.NextDouble() * 5.0;
                double low = Math.Min(open, close) - rand.NextDouble() * 5.0;

                StockPt pt = new StockPt(x, hi, low, open, close, 100000);
                spl.Add(pt);

                open = close;
                // Advance one day
                xDate.AddDays(1.0);
                // but skip the weekends
                if (XDate.XLDateToDayOfWeek(xDate.XLDate) == 6)
                    xDate.AddDays(2.0);
            }
            //添加栅格线
            //myPane.XAxis.MajorGrid.IsVisible = true;
            //myPane.YAxis.MajorGrid.IsVisible = true;
            //myPane.XAxis.MajorGrid.Color = Color.LightGray;
            //myPane.YAxis.MajorGrid.Color = Color.LightGray;
            //myPane.YAxis.MajorGrid.DashOff = 0;
            //myPane.XAxis.MajorGrid.DashOff = 0;


            myPane.XAxis.Type = AxisType.Date;
            myPane.XAxis.Scale.Format = "MM-dd";
            //myPane.XAxis.Scale.FontSpec.Angle = 45;//X轴文字方向，0-90度
            //开始Y轴坐标设置
            ////设置Y轴坐标的范围
            //myPane.YAxis.Scale.Max = Math.Round(maxhi * 1.2, 2);//Math.Ceiling(maxhi);
            //myPane.YAxis.Scale.Min = Math.Round(minlow * 0.8, 2);
            //Y轴最大刻度，注意minStep只会显示刻度线不会显示刻度值
            //myPane.YAxis.Scale.MajorStep = 0.01;
            //myPane.XAxis.Scale.FontSpec.FontColor = Color.Black;
            //myPane.YAxis.Scale.FontSpec.FontColor = Color.Black;

            myPane.XAxis.Type = AxisType.DateAsOrdinal;
            //myPane.Legend.FontSpec.Size = 18f;
            //myPane.Legend.Position = LegendPos.InsideTopRight;
            //myPane.Legend.Location = new Location(0.5f, 0.6f, CoordType.PaneFraction,
            //    AlignH.Right, AlignV.Top);
            JapaneseCandleStickItem myCurve = myPane.AddJapaneseCandleStick("中石化", spl);
            myCurve.Stick.IsAutoSize = true;
            //myCurve.Stick.Color = Color.Blue;
            myCurve.Stick.FallingFill = new Fill(Color.Green);//下跌颜色
            myCurve.Stick.RisingFill = new Fill(Color.Red);//上扬颜色

            // pretty it up a little
            //myPane.Chart.Fill = new Fill(Color.LightBlue, Color.LightGoldenrodYellow, 135.0f);
            //myPane.Fill = new Fill(Color.Orange, Color.FromArgb(220, 220, 255), 45.0f);
            Color c1 = ColorTranslator.FromHtml("#ffffff");
            Color c2 = ColorTranslator.FromHtml("#ffd693");
            myPane.Chart.Fill = new Fill(c1);//图形区域颜色
            myPane.Fill = new Fill(c2);//整体颜色            

            /*
            //成交量线例子
            // Set the Titles
            myPane.Title.Text = "Test Volume Bar";
            myPane.XAxis.Title.Text = "Time";
            myPane.YAxis.Title.Text = "Volume Num";

            // Make up some random data points
            //string[] labels = { "Panther", "Lion", "Cheetah","Cougar", "Tiger", "Leopard" };
            double[] y1 = { 100, 115, 75, 22, 98, 40, -100, -20 };
            double[] y2 = { 90, 100, 95, 35, 80, 35 };
            //double[] y3 = { 80, 110, 65, 15, 54, 67 };
            //double[] y4 = { 120, 125, 100, 40, 105, 75 };

            // Generate a red bar with "Curve 1" in the legend
            BarItem myBar = myPane.AddBar("Curve 1", null, y1, Color.Red);
            myBar.Bar.Fill = new Fill(Color.Red, Color.White, Color.Red);

            // Generate a blue bar with "Curve 2" in the legend
            myBar = myPane.AddBar("Curve 2", null, y2, Color.Blue);
            myBar.Bar.Fill = new Fill(Color.Blue, Color.White, Color.Blue);
            //设置bar宽度
            myPane.BarSettings.ClusterScaleWidth = 0.5;
            log.Info(myPane.BarSettings.GetClusterWidth());
            myPane.BarSettings.Type = ZedGraph.BarType.Cluster;

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
            myPane.XAxis.MajorTic.IsBetweenLabels = true;

            // Set the XAxis labels
            //myPane.XAxis.Scale.TextLabels = labels;
            // Set the XAxis to Text type
            myPane.XAxis.Type = AxisType.Text;

            // Fill the Axis and Pane backgrounds
            myPane.Chart.Fill = new Fill(Color.White,
                  Color.FromArgb(255, 255, 166), 90F);
            myPane.Fill = new Fill(Color.FromArgb(250, 250, 255));
            */

            myPane.AxisChange();
        }
	}
}

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
            this.z1 = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // z1
            // 
            this.z1.IsShowPointValues = false;
            this.z1.Location = new System.Drawing.Point(0, 0);
            this.z1.Name = "z1";
            this.z1.PointValueFormat = "G";
            this.z1.Size = new System.Drawing.Size(1360, 764);
            this.z1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 24);
            this.ClientSize = new System.Drawing.Size(923, 538);
            this.Controls.Add(this.z1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

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

		private void Form1_Load( object sender, System.EventArgs e )
		{
            GraphPane myPane = z1.GraphPane;

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

            myPane.AxisChange();
        }
	}
}

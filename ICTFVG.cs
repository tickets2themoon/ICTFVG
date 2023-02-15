#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.Direct2D1;
using Brush = System.Windows.Media.Brush;
using NinjaTrader.NinjaScript.Indicators.Gemify;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Gemify
{
    public enum FVGFillType
    {
        CLOSE_THROUGH,
        PIERCE_THROUGH
    }

    [Gui.CategoryOrder("Parameters", 1)]
    [Gui.CategoryOrder("Colors", 2)]
    public class ICTFVG : Indicator
    {
        enum FVGType
        {
            R, S
        }

        class FVG
        {
            public double upperPrice;
            public double lowerPrice;
            public string tag;
            public FVGType type;
            public bool filled;
            public DateTime gapStartTime;
            public DateTime fillTime;

            public FVG(string tag, FVGType type, double lowerPrice, double uppperPrice, DateTime gapStartTime)
            {
                this.tag = tag;
                this.type = type;
                this.lowerPrice = lowerPrice;
                this.upperPrice = uppperPrice;
                this.filled = false;
                this.gapStartTime = gapStartTime;
            }
        }

        private List<FVG> fvgList = new List<FVG>();
        private ATR atr;
        private Brush FillBrush;
        private int MIN_BARS_REQUIRED = 3;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Fair Value Gap (ICT)";
                Name = "ICTFVG";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                MaxBars = 1000;

                UpBrush = Brushes.DarkGreen;
                DownBrush = Brushes.Maroon;
                UpAreaBrush = Brushes.DarkGreen;
                DownAreaBrush = Brushes.Maroon;
                FillBrush = Brushes.DimGray;
                ActiveAreaOpacity = 13;
                FilledAreaOpacity = 4;
                ATRPeriod = 10;
                ImpulseFactor = 1.1;
                HideFilledGaps = true;
                FillType = FVGFillType.CLOSE_THROUGH;

            }
            else if (State == State.Configure)
            {
                atr = ATR(ATRPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar <= (Bars.Count - Math.Min(Bars.Count, MaxBars)) + MIN_BARS_REQUIRED) return;

            CheckFilledFVGs();

            // FVG only applies if there's been an impulse move
            if (Math.Abs(High[1] - Low[1]) >= ImpulseFactor * atr.Value[0])
            {
                // Fair value gap while going UP
                // Low[0] > High[2]
                if (Low[0] > High[2])
                {
                    string tag = "FVGUP" + CurrentBar;
                    Draw.Rectangle(this, tag, false, 2, Low[0], -100000, High[2], UpBrush, UpAreaBrush, ActiveAreaOpacity, true);
                    fvgList.Add(new FVG(tag, FVGType.S, High[2], Low[0], Time[2]));
                }
                // Fair value gap while going DOWN
                // High[0] < Low[2]
                if (High[0] < Low[2])
                {
                    string tag = "FVGDOWN" + CurrentBar;
                    Draw.Rectangle(this, "FVGDOWN" + CurrentBar, false, 2, Low[2], -100000, High[0], DownBrush, DownAreaBrush, ActiveAreaOpacity, true);
                    fvgList.Add(new FVG(tag, FVGType.R, High[0], Low[2], Time[2]));
                }
            }
        }

        private void CheckFilledFVGs()
        {
            List<FVG> filled = new List<FVG>();

            foreach (FVG fvg in fvgList)
            {
                if (fvg.filled) continue;

                if (fvg.type == FVGType.R && (FillType == FVGFillType.CLOSE_THROUGH ? (Close[0] >= fvg.upperPrice) : (High[0] >= fvg.upperPrice)))
                {
                    if (DrawObjects[fvg.tag] != null)
                    {
                        fvg.filled = true;
                        fvg.fillTime = Time[0];
                        filled.Add(fvg);
                    }
                }
                else if (fvg.type == FVGType.S && (FillType == FVGFillType.CLOSE_THROUGH ? (Close[0] <= fvg.lowerPrice) : (Low[0] <= fvg.lowerPrice)))
                {
                    if (DrawObjects[fvg.tag] != null)
                    {
                        fvg.filled = true;
                        fvg.fillTime = Time[0];
                        filled.Add(fvg);
                    }
                }

            }

            foreach (FVG fvg in filled)
            {

                if (DrawObjects[fvg.tag] != null)
                {
                    var drawObject = DrawObjects[fvg.tag];
                    Rectangle rect = (Rectangle)drawObject;

                    RemoveDrawObject(fvg.tag);

                    if (!HideFilledGaps)
                    {
                        int startBarsAgo = CurrentBar - Bars.GetBar(fvg.gapStartTime);
                        Brush BorderBrush = fvg.type == FVGType.R ? DownBrush : UpBrush;
                        rect = Draw.Rectangle(this, "FILLEDFVG" + CurrentBar, false, startBarsAgo, fvg.lowerPrice, 0, fvg.upperPrice, BorderBrush, FillBrush, FilledAreaOpacity, true);
                        rect.OutlineStroke.Opacity = Math.Min(100, FilledAreaOpacity * 4);
                    }
                }
                if (HideFilledGaps)
                {
                    fvgList.Remove(fvg);
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(3, int.MaxValue)]
        [Display(Name = "Max Lookback Bars", Order = 100, GroupName = "Parameters")]
        public int MaxBars
        { get; set; }

        [NinjaScriptProperty]
        [Range(3, int.MaxValue)]
        [Display(Name = "ATR Period (To Detect Impulse Moves)", Order = 200, GroupName = "Parameters")]
        public int ATRPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "ATRs in Impulse Move", Order = 300, GroupName = "Parameters")]
        public double ImpulseFactor
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Gap Fill Condition", Order = 325, GroupName = "Parameters")]
        public FVGFillType FillType
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Hide Filled Gaps", Order = 350, GroupName = "Parameters")]
        public bool HideFilledGaps
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish FVG (Border) Color", Order = 100, GroupName = "Colors")]
        public Brush DownBrush
        { get; set; }

        [Browsable(false)]
        public string DownBrushSerializable
        {
            get { return Serialize.BrushToString(DownBrush); }
            set { DownBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish FVG (Area) Color", Order = 110, GroupName = "Colors")]
        public Brush DownAreaBrush
        { get; set; }

        [Browsable(false)]
        public string DownBrushAreaSerializable
        {
            get { return Serialize.BrushToString(DownAreaBrush); }
            set { DownAreaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish FVG (Border) Color", Order = 200, GroupName = "Colors")]
        public Brush UpBrush
        { get; set; }

        [Browsable(false)]
        public string UpBrushSerializable
        {
            get { return Serialize.BrushToString(UpBrush); }
            set { UpBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish FVG (Area) Color", Order = 210, GroupName = "Colors")]
        public Brush UpAreaBrush
        { get; set; }

        [Browsable(false)]
        public string UpAreaBrushSerializable
        {
            get { return Serialize.BrushToString(UpAreaBrush); }
            set { UpAreaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Active Gap Opacity", Order = 300, GroupName = "Colors")]
        public int ActiveAreaOpacity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Filled Gap Opacity", Order = 400, GroupName = "Colors")]
        public int FilledAreaOpacity
        { get; set; }

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Gemify.ICTFVG[] cacheICTFVG;
		public Gemify.ICTFVG ICTFVG(int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			return ICTFVG(Input, maxBars, aTRPeriod, impulseFactor, fillType, hideFilledGaps, downBrush, downAreaBrush, upBrush, upAreaBrush, activeAreaOpacity, filledAreaOpacity);
		}

		public Gemify.ICTFVG ICTFVG(ISeries<double> input, int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			if (cacheICTFVG != null)
				for (int idx = 0; idx < cacheICTFVG.Length; idx++)
					if (cacheICTFVG[idx] != null && cacheICTFVG[idx].MaxBars == maxBars && cacheICTFVG[idx].ATRPeriod == aTRPeriod && cacheICTFVG[idx].ImpulseFactor == impulseFactor && cacheICTFVG[idx].FillType == fillType && cacheICTFVG[idx].HideFilledGaps == hideFilledGaps && cacheICTFVG[idx].DownBrush == downBrush && cacheICTFVG[idx].DownAreaBrush == downAreaBrush && cacheICTFVG[idx].UpBrush == upBrush && cacheICTFVG[idx].UpAreaBrush == upAreaBrush && cacheICTFVG[idx].ActiveAreaOpacity == activeAreaOpacity && cacheICTFVG[idx].FilledAreaOpacity == filledAreaOpacity && cacheICTFVG[idx].EqualsInput(input))
						return cacheICTFVG[idx];
			return CacheIndicator<Gemify.ICTFVG>(new Gemify.ICTFVG(){ MaxBars = maxBars, ATRPeriod = aTRPeriod, ImpulseFactor = impulseFactor, FillType = fillType, HideFilledGaps = hideFilledGaps, DownBrush = downBrush, DownAreaBrush = downAreaBrush, UpBrush = upBrush, UpAreaBrush = upAreaBrush, ActiveAreaOpacity = activeAreaOpacity, FilledAreaOpacity = filledAreaOpacity }, input, ref cacheICTFVG);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Gemify.ICTFVG ICTFVG(int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			return indicator.ICTFVG(Input, maxBars, aTRPeriod, impulseFactor, fillType, hideFilledGaps, downBrush, downAreaBrush, upBrush, upAreaBrush, activeAreaOpacity, filledAreaOpacity);
		}

		public Indicators.Gemify.ICTFVG ICTFVG(ISeries<double> input , int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			return indicator.ICTFVG(input, maxBars, aTRPeriod, impulseFactor, fillType, hideFilledGaps, downBrush, downAreaBrush, upBrush, upAreaBrush, activeAreaOpacity, filledAreaOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Gemify.ICTFVG ICTFVG(int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			return indicator.ICTFVG(Input, maxBars, aTRPeriod, impulseFactor, fillType, hideFilledGaps, downBrush, downAreaBrush, upBrush, upAreaBrush, activeAreaOpacity, filledAreaOpacity);
		}

		public Indicators.Gemify.ICTFVG ICTFVG(ISeries<double> input , int maxBars, int aTRPeriod, double impulseFactor, FVGFillType fillType, bool hideFilledGaps, Brush downBrush, Brush downAreaBrush, Brush upBrush, Brush upAreaBrush, int activeAreaOpacity, int filledAreaOpacity)
		{
			return indicator.ICTFVG(input, maxBars, aTRPeriod, impulseFactor, fillType, hideFilledGaps, downBrush, downAreaBrush, upBrush, upAreaBrush, activeAreaOpacity, filledAreaOpacity);
		}
	}
}

#endregion

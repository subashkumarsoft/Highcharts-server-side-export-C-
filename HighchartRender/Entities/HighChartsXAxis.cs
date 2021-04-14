using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighchartRender.Entities
{
    /// <summary>
    ///  //http://api.highcharts.com/highstock#xAxis.labels
    /// </summary>
    public class HighChartsAxisLabels
    {
        public string align { get; set; }

        public string format { get; set; }
        /// <summary>
        /// 
        /// Callback JavaScript function to format the label. The value is given by this.value. Additional properties for this are axis, chart, isFirst and isLast. Defaults to:
        ////function()
        ///{
        //return this.value;
        ///}
        /// </summary>
        public string formatter { get; set; }

        public string autoRotation { get; set; }

        public string rotation { get; set; }

        public HighChartsStyle style { get; set; } = new HighChartsStyle();

        public int? y { get; set; }

        public int? x { get; set; }
    }

    public class HighChartsStyle
    {
        public string color { get; set; }

        public string fontWeight { get; set; }

        public string fontFamily { get; set; }

        public string fontSize { get; set; }
    }

    public class HighChartsExporting
    {
        public int sourceWidth { get; set; }

        public int sourceHeight { get; set; }

        public int scale { get; set; }


    }

    /// <summary>
    /// http://api.highcharts.com/highstock#xAxis
    /// </summary>
    public abstract class HighChartsAxis
    {
        public HighChartsTitle title { get; set; }

        public HighChartsDateTimeLabelFormats dateTimeLabelFormats { get; set; }

        public virtual HighChartsAxisLabels labels { get; set; }

        public virtual List<HighChartsPlotLines> plotLines { get; set; }

        public string alternateGridColor { get; set; }

        public string lineColor { get; set; }

        public string gridLineColor { get; set; }

        public string gridLineDashStyle { get; set; }

        public virtual int gridLineWidth { get; set; }

        public string minorGridLineColor { get; set; }

        public string minorGridLineDashStyle { get; set; }

        public string minorGridLineWidth { get; set; }

        public string minorTickPosition { get; set; }

        public string minorTickColor { get; set; }

        public int minorTickLength { get; set; }

        public int minorTickWidth { get; set; }

        public string tickColor { get; set; }

        public int tickLength { get; set; }

        public int tickWidth { get; set; }

        public string tickPosition { get; set; }

        public string type { get; set; }

        public double? tickInterval { get; set; }

        public long? min { get; set; }

        public long? max { get; set; }

        public bool opposite { get; set; }
        public object tickPositions { get; set; }

    }

    public class HighChartsXAxis : HighChartsAxis
    {
        public IEnumerable<string> categories { get; set; }
    }

    public class HighChartsYAxis : HighChartsAxis
    {

    }

    public class HighChartsTitle
    {
        public string text { get; set; }
        public string align { get; set; }

        public int? x { get; set; }

        public HighChartsStyle style { get; set; }
    }

    public class HighChartsDateTimeLabelFormats
    {
        public string day { get; set; }
    }

    public class HighChartsPlotLines
    {
        public string color { get; set; } = "#000000";

        public string dashStyle { get; set; }

        public double width { get; set; }

        public double value { get; set; }
        public object label { get; set; }
    }
}

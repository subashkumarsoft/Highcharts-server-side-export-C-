using HighchartRender.Constants;
using System;
using System.Collections;

namespace HighchartRender.Entities
{
    public class HighChartsSeries
    {
        public string type { get; set; } = HighChartType.Default;
        public IEnumerable data { get; set; }
        public string name { get; set; }
        public string color { get; set; }
        public int? yAxis { get; set; }
        public DateTime? date { get; set; }
        public bool showInLegend { get; set; }
        public HighChartsSeriesDataLabels dataLabels { get; set; }
        public int turboThreshold { get; set; }
        public string id { get; set; }
        public string linkedTo { get; set; }
    }

    public class HighChartsSeriesDataLabels
    {
        public bool enabled { get; set; }

        public string color { get; set; }

        public HighChartsStyle style { get; set; }

        public string formatter { get; set; }
    }
}

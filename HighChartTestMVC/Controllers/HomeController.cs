using HighchartRender.Constants;
using HighchartRender.Entities;
using HighchartRender.Services;
using HighchartsExportClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace HighChartTestMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<SeriesValue> _seriesSupplyTempData = new List<SeriesValue>();

            DateTime _date = DateTime.Now;
            for (int i = 0; i <= 10; i++)
            {
                _date = _date.AddHours(1);
                _seriesSupplyTempData.Add(new SeriesValue
                {
                    x = (long)ConvertToUnixTimestamp(_date),
                    y = 10 + i
                });
            }

            List<HighChartsSeries> chillerserieses = new List<HighChartsSeries>();
            chillerserieses.Add(new HighChartsSeries
            {
                date = _date,
                name = "CHWS ",
                type = "line",
                color = "#644c00",
                data = _seriesSupplyTempData,
                yAxis = 0,
                showInLegend = true
            });

            long xAxisMinValue = 0;
            long xAxisMaxValue = 0;
            string title = "Chilled Water Daily Supply subash/ Return Temperature";
            string yAxistitle = "CHW Supply/ Return Temperature(°C)";
            var highchartDat = GetLoadNewTempReadingChart(369, 764, 3, title, "line", xAxisMinValue, xAxisMaxValue, 2, chillerserieses, yAxistitle);
            string imagePath2 = GetFromInternalServer(highchartDat);
            string imagePath = ProcessHighChartImageFromExportServer(highchartDat, title);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }

        public string GetFromInternalServer(HighCharts highChartsData)
        {
            try
            {

                //using (HighChartsRenderServer server = new HighChartsRenderServer())
                using (HighChartsRenderServer server = new HighChartsRenderServer(10000, 4, "127.0.0.1", "3003", true, null))
                {
                    // some highcharte render server needs some times to startup. that's why set a sleep.
                    Thread.Sleep(1000);
                    var response = server.ProcessHighChartsRequest(highChartsData);
                    var chartImageName = string.Format(@"{0}.png", Guid.NewGuid());
                    string outputFile = HttpContext.Server.MapPath("~/ReportResource/" + chartImageName);
                    //string retOutputFile = portalUrl + "ReportResource/HighchartImage/" + chartImageName;

                    //File.WriteAllBytes(outputFile, response);
                    return chartImageName;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string ProcessHighChartImageFromExportServer(HighCharts highCharts, string ChartTitle = "Chart Title")
        {
            try
            {
                var chartImageName = string.Format(@"{0}.jpeg", Guid.NewGuid());
                string outputFile = HttpContext.Server.MapPath("~/ReportResource/" + chartImageName);
                var client = new HighchartsClient("http://export.highcharts.com/");

                var infile = JsonConvert.SerializeObject(highCharts.options, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                JObject jo = new JObject();
                jo = JObject.Parse(infile);

                string title = @"{""text"":" + '"' + ChartTitle + '"' + "}";
                jo.Add("title", JObject.Parse(title));
                JObject header = (JObject)jo.SelectToken("chart");

                header.Property("marginRight").Remove();
                header.Property("marginLeft").Remove();
                //jo.Property("marginLeft").Remove();
                infile = jo.ToString();
                //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/Phantom/jsondata.json"), infile);
                Thread.Sleep(14000);
                var res = client.GetChartImageLinkFromOptionsAsync(infile).Result;

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(res), outputFile);
                }
                //logger.Error(chartImageName + " Image Generated!");
                return chartImageName;
            }
            catch (Exception ex)
            {
                //logger.Error(ex.InnerException + "Process Image--");
                throw;
            }

        }

        private HighCharts GetLoadNewTempReadingChart(int imageHeight, int imageWidth, int imageScale, string chartTitle, string chartType, long xAxisMinValue, long xAxisMaxValue, double? yAxisInterval, List<HighChartsSeries> series, string yAxistitle)
        {
            try
            {
                var highChartsStyle = new HighChartsStyle() { color = "#000000", fontWeight = "normal" };

                return new HighCharts()
                {
                    async = false,
                    content = "options",
                    title = new HighChartsTitle()
                    {
                        text = chartTitle,
                        x = -20, //center
                        style = new HighChartsStyle()
                        {
                            fontSize = "16pt",
                        }
                    },

                    options = new HighChartsOptions(chartType)
                    {
                        credits = {
                            enabled = false
                        },
                        boost = {
                            enabled = false,
                        },
                        lang = {
                            noData = "No data to display in this date range"
                        },
                        noData = new HighChartsNoData()
                        {
                            style = new HighChartsStyle()
                            {
                                fontWeight = "normal",
                                fontSize = "18px",
                                color = "#707070"
                            }
                        },
                        exporting = new HighChartsExporting
                        {
                            sourceWidth = imageWidth,
                            sourceHeight = imageHeight,
                            scale = imageScale
                        },
                        xAxis = new HighChartsXAxis()
                        {
                            title = new HighChartsTitle()
                            {
                                //text = "Time (HH:MM DD/MM/YYYY)"
                                text = "Time (hh:mm)"
                            },
                            type = "datetime",
                            dateTimeLabelFormats = new HighChartsDateTimeLabelFormats()
                            {
                                day = "%H:%M"
                            },
                            tickInterval = 3600 * 1000,
                            min = xAxisMinValue,
                            labels = new HighChartsAxisLabels()
                            {
                                y = 30,
                                style = new HighChartsStyle()
                                {
                                    fontSize = "9px !important"
                                }
                            }
                        },
                        plotOptions = new HighChartsPlotOptions()
                        {
                            line = new HighChartsLinePlotOptions()
                            {
                                marker = new HighChartsMarker()
                                {
                                    enabled = false
                                }
                            },
                            series = new HighChartsSeriesPlotOptions()
                            {
                                marker = new HighChartsMarker()
                                {
                                    enabled = false
                                },
                                turboThreshold = 0
                            }
                        },
                        yAxis = new HighChartsYAxis()
                        {
                            gridLineWidth = 1,
                            labels = new HighChartsAxisLabels()
                            {
                                align = "center",
                                format = "{value:.0f}",
                                formatter = null,
                                autoRotation = "[-45]",
                                rotation = "",
                                style = highChartsStyle,
                                y = -10,
                                x = null,

                            },
                            tickInterval = yAxisInterval,
                            gridLineColor = "#8C8C8C",
                            title = new HighChartsTitle()
                            {
                                text = yAxistitle,//"Chiller Power (kW)",
                                //x = 10//-20
                            },
                        },
                        legend = new HighChartsLegend()
                        {
                            layout = "horizontal",
                            //x = 30,
                            align = "center",
                            verticalAlign = "bottom"
                        },
                        series = series,
                    },
                    type = HighChartsExportFormat.png,
                    width = 500,
                    scale = null,
                    constr = null
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }

    public class SeriesValue
    {
        public long x { get; set; }
        public double? y { get; set; }
    }
}
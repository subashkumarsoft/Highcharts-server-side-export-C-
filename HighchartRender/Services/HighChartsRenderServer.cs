﻿using HighchartRender.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Web;

namespace HighchartRender.Services
{
    internal class HighChartsRenderServerWrapper
    {
        public string infile { get; set; }
        public string constr { get; set; }
        public string outfile { get; set; }

        public string callback { get; set; }
    }

    /// <summary>
    /// Needed to run phantomjs highcharts-convert.js -host 127.0.0.1 -port 3003
    /// https://github.com/highslide-software/highcharts.com/blob/master/exporting-server/phantomjs/highcharts-convert.js
    /// </summary>
    public class HighChartsRenderServer : IDisposable
    {
        private readonly bool _keepAlive;
        private string _port;
        private string _ip;
        private JsonSerializerSettings _jsonSerializerSettings;
        /// <summary>
        /// Timeout before request should be retried
        /// </summary>
        private int TimeoutBeforeRetry;

        /// <summary>
        /// How many times should the request to generate the chart be retried?
        /// </summary>
        private int Maxretries { get; set; }

        public string TemporaryImagesDirectory { get; set; }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
      

        private Process ExeProcess { get; set; }
        public List<string> CreatedTempFiles { get; set; }

        //public HighChartsRenderServer(int timeoutBeforeRetry = 10000, int maxRetries = 4, string ip = "127.0.0.1", string port = "3003", bool keepAlive = true, string temporaryFolder = null)  
            public HighChartsRenderServer(int timeoutBeforeRetry , int maxRetries , string ip , string port, bool keepAlive , string temporaryFolder)
        {
            
                
                _keepAlive = keepAlive;
                InitServerSerializerAndTmpFileList(ip, port);

                TemporaryImagesDirectory = temporaryFolder ?? Path.GetTempPath();

                StartServer();
                TimeoutBeforeRetry = timeoutBeforeRetry;
                Maxretries = maxRetries;
        }

        private void StartServer()
        {
            try
            {
                var phantomDirectory = HttpContext.Current.Server.MapPath("~/App_Data/phantomjs/");
                var pathRooth = Path.GetPathRoot(phantomDirectory);

                var highChartsConvertJsFile = Path.Combine(phantomDirectory, "highcharts-convert.js");

                string arguments =
                    $" -host {_ip} -port {_port}";

                InitializeCommandProcess();

                using (StreamWriter sw = ExeProcess.StandardInput)
                {
                    if (!sw.BaseStream.CanWrite) return;
                    sw.WriteLine($"cd \"{phantomDirectory}\"");
                    sw.WriteLine($"{pathRooth.Replace("\\", "")}");
                    string format = $"phantomjs.exe \"{highChartsConvertJsFile}\" {arguments}";
                    sw.WriteLine(format);
                    var readLine = ExeProcess.StandardOutput.ReadLine();
                    Console.WriteLine(readLine);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.InnerException);
                Console.ReadLine();
            }
           
        }

        private void InitializeCommandProcess()
        {
            ExeProcess = new Process();
            var info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Verb = "runas"
            };
            ExeProcess.StartInfo = info;
            ExeProcess.Start();
        }

        private void InitServerSerializerAndTmpFileList(string ip, string port)
        {
            _jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            _port = port;
            _ip = ip;
            CreatedTempFiles = new List<string>();
        }

        public byte[] ProcessHighChartsRequest(HighCharts chart)
        {
            if (ExeProcess.HasExited)
            {
                if (_keepAlive)
                {
                    StartServer();
                }
                else
                {
                    Dispose();
                    throw new Exception("could not process.  Server is no longer running and KeepAlive is set to false.");
                }
            }
            var newGuid = Guid.NewGuid();

            var temporaryGraphImageFile = Path.Combine(TemporaryImagesDirectory, $"{newGuid}.png");

            var wrapper = new HighChartsRenderServerWrapper()
            {
                infile = JsonConvert.SerializeObject(chart.options,
                Formatting.Indented,
                _jsonSerializerSettings),
                constr = "Chart",
                outfile = temporaryGraphImageFile,
                callback = chart.callback
            };

            return PostRequestAndReturnBytesOnSuccess(wrapper, temporaryGraphImageFile);
        }

        private byte[] PostRequestAndReturnBytesOnSuccess(HighChartsRenderServerWrapper wrapper, string temporaryGraphImageFile)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://{_ip}:{_port}");


            request.ContentType = "application/json";


            request.Timeout = isFirstRun() ? 20000 : TimeoutBeforeRetry;
            request.Method = "POST";

            var postData = JsonConvert.SerializeObject(wrapper, _jsonSerializerSettings);
            if (isFirstRun())
            {
                Thread.Sleep(350);
            }

            bool sucess = false;
            var retries = 0;

            while (retries > Maxretries || sucess != true)
            {
                try
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(postData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var bytesToReturn = File.ReadAllBytes(temporaryGraphImageFile);
                            CreatedTempFiles.Add(temporaryGraphImageFile);
                            return bytesToReturn;
                        }
                    }
                    sucess = true;
                }
                catch (Exception ex)
                {
                    if (retries + 1 == Maxretries)
                    {
                        throw;
                    }
                    retries++;
                }
            }
            return null;
        }


        private bool isFirstRun()
        {
            return CreatedTempFiles == null || CreatedTempFiles.Count == 0;
        }

        public void Dispose()
        {
            foreach (var createdTempFile in CreatedTempFiles)
            {
                File.Delete(createdTempFile);
            }
            if (!ExeProcess.HasExited)
            {
                ExeProcess.Kill();
            }
            KillPhantomJs();
        }
        private static void KillPhantomJs()
        {
            foreach (var process in Process.GetProcessesByName("phantomjs"))
            {
                process.Kill();
            }
        }
    }
}

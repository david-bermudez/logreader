using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Configuration;
using NLog;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace LogReader.Controllers
{
    public class DataController : ApiController
    {
        readonly string urlAPI = "http://10.101.100.41:9001/MatrizApi/logFileName?logDate=20220707";
        readonly string versionAPI = "v1.0";
        Logger log = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [Route("log/list")]
        public string Get()
        {

            string data = GetLogDataFromAPI();
            JArray logList = JArray.Parse(data);
            for (int i = 0; i < logList.Count; i++) 
            {
                JObject item = (JObject) logList[i] ;
                string fullPath = item["fullName"].ToString();

                //LECTURA DE CADA ARCHIVO
            }
            

            return data;
        }

        private string GetLogDataFromAPI()
        {
            WebRequest request = WebRequest.Create(urlAPI);
            request.Method = "GET";
            WebResponse webResponse = request.GetResponse();
            Stream webStream = webResponse.GetResponseStream();
            StreamReader reader = new StreamReader(webStream);
            string data = reader.ReadToEnd();

            return data;
        }

        private void Info(string text)
        {
            string trace = "N";

            try
            {
                trace = WebConfigurationManager.AppSettings["Trace"];
            }
            catch (Exception)
            {
                trace = "N";
            }

            if (trace != null)
            {
                if (trace.Equals("Y"))
                {
                    log.Info(versionAPI + ";" + text.Replace("\r\n", " ").Replace("\n", " "));
                }
            }
        }

    }

}
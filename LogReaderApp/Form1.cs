using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogReaderApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Get();
        }

        public string Get()
        {
            listBox1.Items.Clear();
            string data = GetLogDataFromAPI();
            Newtonsoft.Json.Linq.JArray logList = Newtonsoft.Json.Linq.JArray.Parse(data);
            for (int i = 0; i < logList.Count; i++)
            {
                Newtonsoft.Json.Linq.JObject item = (Newtonsoft.Json.Linq.JObject)logList[i];
                string fullPath = item["fullName"].ToString();
                listBox1.Items.Add(fullPath);
            }


            return data;
        }

        private string GetLogDataFromAPI()
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create("http://10.101.100.41:9001/MatrizApi/logFileName?logDate=20220707");
            request.Method = "GET";
            System.Net.WebResponse webResponse = request.GetResponse();
            System.IO.Stream webStream = webResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(webStream);
            string data = reader.ReadToEnd();

            return data;
        }
    }

   
}

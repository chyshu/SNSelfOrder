using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder 
{ 
    public class PrinterSetting
    {
        string sid = "";
        string pos_code = "";
        string customerid = "";
        string store_code = "";
        string device_type = "";
        string config_type = "";
        string port = "";
        string buad = "";
        string data_format = "";
        string handshake = "";
        string device_name = "";
        string map_printer = "";
        string ip_address = "";
        string del_flag = "";

        public string Pos_code { get => pos_code; set => pos_code = value; }
        public string Device_type { get => device_type; set => device_type = value; }
        public string Config_type { get => config_type; set => config_type = value; }
        public string Port { get => port; set => port = value; }
        public string Buad { get => buad; set => buad = value; }
        public string Data_format { get => data_format; set => data_format = value; }
        public string Handshake { get => handshake; set => handshake = value; }
        public string Device_name { get => device_name; set => device_name = value; }
        public string Map_printer { get => map_printer; set => map_printer = value; }
        public string Ip_address { get => ip_address; set => ip_address = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
    }
}

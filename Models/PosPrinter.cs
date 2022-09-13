using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Models
{
    
    public class PosPrinter
    { 

        string printerName = "";
        string deviceName = "";
        string deviceType = "";
        string port = "";
        bool isDefault = false;

        public string PrinterName { get => printerName; set => printerName = value; }
        public string DeviceName { get => deviceName; set => deviceName = value; }
        public string Port { get => port; set => port = value; }
        public bool IsDefault { get => isDefault; set => isDefault = value; }
        public string DeviceType { get => deviceType; set => deviceType = value; }
    }
}

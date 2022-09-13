using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Models
{
    public class Station
    {
        public string Sid { get; set; }
        public string CustomerID { get; set; }

        public string Store_code { get; set; }
        public string Pos_code { get; set; }
        public DateTime? Expiry_date { get; set; }
        public DateTime? Last_checked { get; set; }
        public string MAC { get; set; }
        public DateTime? Registed_date { get; set; }
        public string Connection { get; set; }

        public string Token { get; set; }

        public string stationid { get; set; }

        public string Accesscode { get; set; }

        public string Vcode { get; set; }

        public string Pos_name { get; set; }


        public string Set_code { get; set; }
        public string Config_code { get; set; }
        public string Pricecolumn1 { get; set; }
        public string Pricecolumn2 { get; set; }
        public string Pricecolumn3 { get; set; }
        public string Pricecolumn4 { get; set; }
        public string PosID { get; set; }
        public bool IsAuth { get; set; }
        public string SalePriceColumn { get; set; }
        public string TakeawayPriceColumn { get; set; }
        public int StatusCode  { get; set; }
}
}

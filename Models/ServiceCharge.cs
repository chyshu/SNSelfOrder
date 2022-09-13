using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class ServiceCharge
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string charge_type = "";
        string charge_name = "";
        string bdate = "";
        string edate = "";
        string charge_flag = "";
        string charge_item = "";
        string variety_code = "";
        string charge_rate = "";
        string del_flag = "";
        string upd_date = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Charge_type { get => charge_type; set => charge_type = value; }
        public string Charge_name { get => charge_name; set => charge_name = value; }
        public string Bdate { get => bdate; set => bdate = value; }
        public string Edate { get => edate; set => edate = value; }
        public string Charge_flag { get => charge_flag; set => charge_flag = value; }
        public string Charge_item { get => charge_item; set => charge_item = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Charge_rate { get => charge_rate; set => charge_rate = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
    }
}

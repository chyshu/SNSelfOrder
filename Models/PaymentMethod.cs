using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    class PaymentMethod
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string tender_code = "";
        string tender_name = "";
        string display_name = "";
        string over_flag = "";
        string over_max = "";
        string eftpos_flag = "";
        string paymachine_flag = "";
        string received_flag = "";
        string change_flag = "";
        string disp_flag = "";
        string disp_order = "";
        string del_flag = "";
        string print_at_kitchen = "";

        string card_charge_item = "";
        string card_charge_rate = "";
        string card_charge_flag = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Tender_code { get => tender_code; set => tender_code = value; }
        public string Tender_name { get => tender_name; set => tender_name = value; }
        public string Display_name { get => display_name; set => display_name = value; }
        public string Over_flag { get => over_flag; set => over_flag = value; }
        public string Over_max { get => over_max; set => over_max = value; }
        public string Eftpos_flag { get => eftpos_flag; set => eftpos_flag = value; }
        public string Paymachine_flag { get => paymachine_flag; set => paymachine_flag = value; }
        public string Received_flag { get => received_flag; set => received_flag = value; }
        public string Change_flag { get => change_flag; set => change_flag = value; }
        public string Disp_flag { get => disp_flag; set => disp_flag = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }

        public string Card_charge_item { get => card_charge_item; set => card_charge_item = value; }
        public string Card_charge_rate { get => card_charge_rate; set => card_charge_rate = value; }
        public string Card_charge_flag { get => card_charge_flag; set => card_charge_flag = value; }
        public string Print_at_kitchen { get => print_at_kitchen; set => print_at_kitchen = value; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class PosModifier
    {
        string sid = "";
        string customerid = "";
        string modifier_code = "";
        string description = "";
        string caption = "";
        string caption_fn = "";
        string image = "";
        string del_flag = "";
        string upd_date = "";
        string disp_caption = "";
        string disp_price = "";
        public string Customerid { get => customerid; set => customerid = value; }
        public string Modifier_code { get => modifier_code; set => modifier_code = value; }
        public string Description { get => description; set => description = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Image { get => image; set => image = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Sid { get => sid; set => sid = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Disp_caption { get => disp_caption; set => disp_caption = value; }
        public string Disp_price { get => disp_price; set => disp_price = value; }
    }
}

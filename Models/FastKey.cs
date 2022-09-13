using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class FASTKeySet
    {
        string sid = "";
        string customerid = "";
        string location = "";
        string store_code = "";
        string set_code = "";
        string set_name = "";
        string del_flag = "";
        string upd_date = "";
        List<FASTKey> fastKeyView = new List<FASTKey>();
        public string Sid { get => sid; set => sid = value; }
        public string Set_code { get => set_code; set => set_code = value; }
        public string Set_name { get => set_name; set => set_name = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public List<FASTKey> FastKeys { get => fastKeyView; set => fastKeyView = value; }
        public string CustomerID { get => customerid; set => customerid = value; }
        public string Location { get => location; set => location = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Store_code { get => store_code; set => store_code = value; }
    }
    public class FASTKey
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string set_code = "";
        string psid = "";
        string caption = "";
        string caption2 = "";
        string caption3 = "";
        string priceLine = "";
        string op_code = "";
        string ref_code = "";
        string width = "";
        string height = "";
        string display_yn = "";
        string default_yn = "";
        string caption_yn = "";
        string caption2_yn = "";
        string caption3_yn = "";
        string fontcolor = "";
        string bgcolor = "";
        string fontfamily = "";
        string fontsize = "";
        string fontstyle = "";
        string imagefile = "";
        string fullimage_yn = "";
        string textbgcolor = "";
        string textheight = "";
        string disp_order = "";
        string del_flag = "";
        string upd_date = "";
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Set_code { get => set_code; set => set_code = value; }
        public string Psid { get => psid; set => psid = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Op_code { get => op_code; set => op_code = value; }
        public string Ref_code { get => ref_code; set => ref_code = value; }
        public string Width { get => width; set => width = value; }
        public string Height { get => height; set => height = value; }
        public string Display_yn { get => display_yn; set => display_yn = value; }
        public string Default_yn { get => default_yn; set => default_yn = value; }
        public string Caption_yn { get => caption_yn; set => caption_yn = value; }
        public string Caption2_yn { get => caption2_yn; set => caption2_yn = value; }
        public string Caption3_yn { get => caption3_yn; set => caption3_yn = value; }
        public string Fontcolor { get => fontcolor; set => fontcolor = value; }
        public string Bgcolor { get => bgcolor; set => bgcolor = value; }
        public string Fontfamily { get => fontfamily; set => fontfamily = value; }
        public string Fontsize { get => fontsize; set => fontsize = value; }
        public string Fontstyle { get => fontstyle; set => fontstyle = value; }
        public string Imagefile { get => imagefile; set => imagefile = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Fullimage_yn { get => fullimage_yn; set => fullimage_yn = value; }
        public string TextBGColor { get => textbgcolor; set => textbgcolor = value; }
        public string TextHeight { get => textheight; set => textheight = value; }
        public string Caption2 { get => caption2; set => caption2 = value; }
        public string Caption3 { get => caption3; set => caption3 = value; }
        public string PriceLine { get => priceLine; set => priceLine = value; }
    }


}

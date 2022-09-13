using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class PosItem
    {
        string customerid = "";
        string item_code = "";
        string item_type = "";
        string item_kind = "";
        string item_name = "";
        string item_name_fn = "";
        string description = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string gst = "";
        string cate_code = "";
        string mod_code = "";
        string set_code = "";
        string kitchen_name = "";
        string kitchen_name_f = "";
        string kitchen_remark = "";
        string dept = "";
        string buttonid = "";
        string disp_order = "";
        string p1 = "";
        string p2 = "";
        string p3 = "";
        string p4 = "";
        string p5 = "";
        string p6 = "";
        string m1 = "";
        string m2 = "";
        string m3 = "";
        string s1 = "";
        string s2 = "";
        string s3 = "";
        string s4 = "";
        string s5 = "";
        string s6 = "";
        string s7 = "";
        string s8 = "";
        string s9 = "";
        string spicy = "";
        string vegetarian = "";
        string beef = "";
        string pork = "";
        string basic_item = "";
        string modisetid = "";
        string rest_usr = "";
        string del_flag = "";
        string upd_date = "";
        string printer_name = "";
        string imagefile = "";
        string kds_name = "";
        string mpoint = "";
        string takefee = "";
        string ubereatsfee = "";
        string soldout = "";

        public string Customerid { get => customerid; set => customerid = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Item_type { get => item_type; set => item_type = value; }
        public string Item_kind { get => item_kind; set => item_kind = value; }
        public string Item_name { get => item_name; set => item_name = value; }
        public string Item_name_fn { get => item_name_fn; set => item_name_fn = value; }
        public string Description { get => description; set => description = value; }
        public string Gst { get => gst; set => gst = value; }
        public string Cate_code { get => cate_code; set => cate_code = value; }
        public string Mod_code { get => mod_code; set => mod_code = value; }
        public string Set_code { get => set_code; set => set_code = value; }
        public string Kitchen_name { get => kitchen_name; set => kitchen_name = value; }
        public string Kitchen_name_f { get => kitchen_name_f; set => kitchen_name_f = value; }
        public string Kitchen_remark { get => kitchen_remark; set => kitchen_remark = value; }
        public string Dept { get => dept; set => dept = value; }
        public string Buttonid { get => buttonid; set => buttonid = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string P1 { get => p1; set => p1 = value; }
        public string P2 { get => p2; set => p2 = value; }
        public string P3 { get => p3; set => p3 = value; }
        public string P4 { get => p4; set => p4 = value; }
        public string P5 { get => p5; set => p5 = value; }
        public string P6 { get => p6; set => p6 = value; }
        public string M1 { get => m1; set => m1 = value; }
        public string M2 { get => m2; set => m2 = value; }
        public string M3 { get => m3; set => m3 = value; }
        public string S1 { get => s1; set => s1 = value; }
        public string S2 { get => s2; set => s2 = value; }
        public string S3 { get => s3; set => s3 = value; }
        public string S4 { get => s4; set => s4 = value; }
        public string S5 { get => s5; set => s5 = value; }
        public string S6 { get => s6; set => s6 = value; }
        public string S7 { get => s7; set => s7 = value; }
        public string S8 { get => s8; set => s8 = value; }
        public string S9 { get => s9; set => s9 = value; }
        public string Spicy { get => spicy; set => spicy = value; }
        public string Vegetarian { get => vegetarian; set => vegetarian = value; }
        public string Beef { get => beef; set => beef = value; }
        public string Pork { get => pork; set => pork = value; }
        public string Basic_item { get => basic_item; set => basic_item = value; }
        public string Modisetid { get => modisetid; set => modisetid = value; }
        public string Rest_usr { get => rest_usr; set => rest_usr = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Printer_name { get => printer_name; set => printer_name = value; }
        public string Imagefile { get => imagefile; set => imagefile = value; }
        public string Kds_name { get => kds_name; set => kds_name = value; }
        public string Mpoint { get => mpoint; set => mpoint = value; }
        public string Takefee { get => takefee; set => takefee = value; }
        public string Ubereatsfee { get => ubereatsfee; set => ubereatsfee = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Soldout { get => soldout; set => soldout = value; }
    }
    public class ItemSize
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string size_code = "";
        string caption = "";
        string caption_fn = "";
        string description = "";
        string amount = "";
        string disp_order = "";
        string del_flag = "";
        string next_modset = "";
        string upd_date = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Size_code { get => size_code; set => size_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Description { get => description; set => description = value; }
        public string Amount { get => amount; set => amount = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Store_code { get => store_code; set => store_code = value; }
    }

    public class ItemVariety
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string variety_code = "";
        string caption = "";
        string caption_fn = "";
        string description = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string disp_order = "";
        string del_flag = "";
        string next_modset = "";
        string upd_date = "";
        string size_code = "";
        string cook_type = "";
        string default_item = "";
        string kitchen_name = "";
        string kitchen_name_fn = "";
        string soldout = "";
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Description { get => description; set => description = value; }

        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Size_code { get => size_code; set => size_code = value; }
        public string Cook_type { get => cook_type; set => cook_type = value; }
        public string Default_item { get => default_item; set => default_item = value; }
        public string Kitchen_name { get => kitchen_name; set => kitchen_name = value; }
        public string Kitchen_name_fn { get => kitchen_name_fn; set => kitchen_name_fn = value; }
        public string Soldout { get => soldout; set => soldout = value; }
    }
    public class Promotions
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string size_code = "";
        string variety_code = "";
        string caption = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string bdate = "";
        string edate = "";
        string btime = "";
        string etime = "";
        string w1 = "";
        string w2 = "";
        string w3 = "";
        string w4 = "";
        string w5 = "";
        string w6 = "";
        string w7 = "";
        string daily = "";

        string del_flag = "";
        string upd_date = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Size_code { get => size_code; set => size_code = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Bdate { get => bdate; set => bdate = value; }
        public string Edate { get => edate; set => edate = value; }
        public string Btime { get => btime; set => btime = value; }
        public string Etime { get => etime; set => etime = value; }
        public string W1 { get => w1; set => w1 = value; }
        public string W2 { get => w2; set => w2 = value; }
        public string W3 { get => w3; set => w3 = value; }
        public string W4 { get => w4; set => w4 = value; }
        public string W5 { get => w5; set => w5 = value; }
        public string W6 { get => w6; set => w6 = value; }
        public string W7 { get => w7; set => w7 = value; }
        public string Daily { get => daily; set => daily = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
    }
    public class MakrupItem
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string markup_type = "";
        string item_code = "";
        string variety_code = "";
        string message = "";
        string disp_order = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";

        string del_flag = "";
        string upd_date = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Markup_type { get => markup_type; set => markup_type = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Message { get => message; set => message = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
    }
    public class PsCategory
    {
        string sid = "";
        string customerid = "";
        string cate_code = "";
        string cate_type = "";
        string cate_name = "";
        string cate_fn_name = "";
        string disp_order = "";
        string buttonid = "";
        string publish = "";
        string pcate_code = "";
        string del_flag = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Cate_code { get => cate_code; set => cate_code = value; }
        public string Cate_type { get => cate_type; set => cate_type = value; }
        public string Cate_name { get => cate_name; set => cate_name = value; }
        public string Cate_fn_name { get => cate_fn_name; set => cate_fn_name = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Buttonid { get => buttonid; set => buttonid = value; }
        public string Publish { get => publish; set => publish = value; }
        public string Pcate_code { get => pcate_code; set => pcate_code = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
    }
}

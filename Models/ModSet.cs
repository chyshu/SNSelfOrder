using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    /*
     used to download procedure
     */
    public class ModSetClass
    {
        string sid = "";
        string customerid = "";
        string modset_code = "";
        string caption = "";
        string caption_fn = "";
        string del_flag = "";
        string upd_date = "";
        string mod_type = "";
        string amount = "";
        string max = "";
        string min = "";
        string next_modset = "";

        List<ModSetTiClass> modSetlist = new List<ModSetTiClass>();
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Modset_code { get => modset_code; set => modset_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }
        public string Amount { get => amount; set => amount = value; }
        public string Max { get => max; set => max = value; }
        public string Min { get => min; set => min = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public List<ModSetTiClass> ModSetlist { get => modSetlist; set => modSetlist = value; }
    }
    public class ModSetTiClass
    {
        string sid = "";
        string psid = "";
        string modifier_code = "";
        string caption = "";
        string caption_fn = "";
        string del_flag = "";
        string upd_date = "";
        string mod_type = "";
        string price_type = "";
        string amount = "";
        string max = "";
        string min = "";
        string next_modset = "";
        string image = "";
        string disp_caption = "";
        string disp_price = "";
        string soldOut = "";
        public string Sid { get => sid; set => sid = value; }
        public string Psid { get => psid; set => psid = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }
        public string Price_type { get => price_type; set => price_type = value; }
        public string Amount { get => amount; set => amount = value; }
        public string Max { get => max; set => max = value; }
        public string Min { get => min; set => min = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public string Image { get => image; set => image = value; }
        public string Modifier_code { get => modifier_code; set => modifier_code = value; }
        public string Disp_caption { get => disp_caption; set => disp_caption = value; }
        public string Disp_price { get => disp_price; set => disp_price = value; }
        public string SoldOut { get => soldOut; set => soldOut = value; }
    }

    /* use for Kiosk Opeate */
    public class ModSet
    {
        string sid = "";
        string customerid = "";
        string modset_code = "";
        string caption = "";
        string caption_fn = "";
        string del_flag = "";
        string upd_date = "";
        string mod_type = "";
        string amount = "";
        string max = "";
        string min = "";
        string next_modset = "";
        string fontFamily = "Arial";
        int fontSize = 12;
        string fontStyle = "Regular";
        bool isVisible = true;

        List<ModSetTi> modSetlist = new List<ModSetTi>();
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Modset_code { get => modset_code; set => modset_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }
        public string Amount { get => amount; set => amount = value; }
        public string Max { get => max; set => max = value; }
        public string Min { get => min; set => min = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }

        public string FontFamily { get => fontFamily; set => fontFamily = value; }
        public int FontSize { get => fontSize; set => fontSize = value; }
        public string FontStyle { get => fontStyle; set => fontStyle = value; }

        public bool IsVisible { get => isVisible; set => isVisible = value; }
        public List<ModSetTi> ModSetlist { get => modSetlist; set => modSetlist = value; }        
    }
    public class ModSetTi : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string sid = "";
        string psid = "";
        string modifier_code = "";
        string caption = "";
        string caption_fn = "";
        string del_flag = "";
        string upd_date = "";
        string mod_type = "";
        string price_type = "";
        string amount = "";
        string max = "";
        string min = "";
        string next_modset = "";
        string image = "";
        string disp_caption = "";
        string disp_price = "";
        string soldOut = "";
        string strSoldOut = "";
        string str_upd_date = "";
        public string Sid { get => sid; set => sid = value; }
        public string Psid { get => psid; set => psid = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }
        public string Price_type { get => price_type; set => price_type = value; }
        public string Amount { get => amount; set => amount = value; }
        public string Max { get => max; set => max = value; }
        public string Min { get => min; set => min = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public string Image { get => image; set => image = value; }
        public string Modifier_code { get => modifier_code; set => modifier_code = value; }
        public string Disp_caption { get => disp_caption; set => disp_caption = value; }
        public string Disp_price { get => disp_price; set => disp_price = value; }
        public string SoldOut { get => soldOut; set { soldOut = value; OnPropertyChanged("SoldOut"); } }

        public string StrSoldOut { get => strSoldOut; set { strSoldOut = value; OnPropertyChanged("StrSoldOut"); } }
        public string Str_upd_date { get => str_upd_date; set => str_upd_date = value; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    public class ItemMod
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string variety_code = "";
        string modset_code = "";
        string disp_step = "";
        string disp_order = "";
        string del_flag = "";
        string upd_date = "";

        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Modset_code { get => modset_code; set => modset_code = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Disp_step { get => disp_step; set => disp_step = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
    }
}


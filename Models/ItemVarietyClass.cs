using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRPos.Data
{
    [Serializable()]
    public class ItemVarietyClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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
        string fontcolor = "#FFFFFF";
        string bgColor = "#dddddd";
        string fontFamily = "";
        int fontSize = 16;
        string fontStyle = "";
        string picture = "";
        string textBgColor = "Transparent";
        int offset = 10;

        string full_Image_yn = "N";
        string kitchen_name = "";
        string kitchen_name_fn = "";
        bool isSelected = false;

        int varietyWidth = 140;
        int varietyHeight = 70;

        private DataRow mVarietyRow = null;

        private ObservableCollection<PsModset01> mModifierSets = new ObservableCollection<PsModset01>();

        public ObservableCollection<PRPos.Data.PsModset01> ModifierSets
        {
            get { return mModifierSets; }
            set { mModifierSets = value; OnPropertyChanged("ModifierSets"); }
        }
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
        public string FontColor { get => fontcolor; set => fontcolor = value; }
        public string BackColor { get => bgColor; set => bgColor = value; }
        public string FontFamily { get => fontFamily; set => fontFamily = value; }
        public int FontSize { get => fontSize; set => fontSize = value; }
        public string FontStyle { get => fontStyle; set => fontStyle = value; }
        public string Picture { get => picture; set => picture = value; }
        public string TextBgColor { get => textBgColor; set => textBgColor = value; }
        public string Full_Image_yn { get => full_Image_yn; set => full_Image_yn = value; }
        public int Offset { get => offset; set => offset = value; }
        public string Default_item { get => default_item; set => default_item = value; }
        public string Kitchen_name { get => kitchen_name; set => kitchen_name = value; }
        public string Kitchen_name_fn { get => kitchen_name_fn; set => kitchen_name_fn = value; }

        public DataRow VarietyRow { get => mVarietyRow; set => mVarietyRow = value; }
        public bool IsSelected { get => isSelected; set { isSelected = value; 
                //Debug.WriteLine("IsSelected ItemVariety " + value); 
                OnPropertyChanged(""); } }

        public int VarietyWidth { get => varietyHeight; set { varietyWidth = value; OnPropertyChanged("VarietyWidth"); } }
        public int VarietyHeight
        {
            get => varietyHeight; set { varietyHeight = value; OnPropertyChanged("VarietyHeight"); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            //Debug.WriteLine("OnPropertyChanged ItemVariety ");
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text; 
using System.Threading.Tasks;

namespace PRPos.Data
{
    [Serializable()]
    public class PsModset01 : INotifyPropertyChanged
    {
        public PsModset01()
        {
            mModifier = new ObservableCollection<PsModsetDT>();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string variety_code = "";
        string caption = "";
        string caption_fn = "";
        string mod_type = "";
        string modSet_code = "";
        decimal  amount = 0;
        int max_selection = 0;
        int min_selection = 0;
        string next_modset = "";
        public bool IsVisible { get => true; }
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }
        public decimal Amount { get => amount; set => amount = value; }
        public int Max_selection { get => max_selection; set => max_selection = value; }
        public int Min_selection { get => min_selection; set => min_selection = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }
        public string ModSet_code { get => modSet_code; set => modSet_code = value; }

        private ObservableCollection<PsModsetDT> mModifier;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            //Debug.WriteLine("OnPropertyChanged PsModset01 ");
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public ObservableCollection<PRPos.Data.PsModsetDT> Modifiers
        {
            get { return mModifier; }
            set { mModifier = value; OnPropertyChanged("PsModset01"); }
        }


    }

    [Serializable()]
    public class PsModsetDT : INotifyPropertyChanged
    {
        private PRPos.Data.PsModset01 modifierSet;
        public PsModsetDT(PRPos.Data.PsModset01 set)
        {
            modifierSet = set;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        string sid = "";
        string psid = "";        
        string modifier_code = "";               
        string caption = "";
        string caption_fn = "";
        string mod_type = "";
        string price_type = "";        
        decimal sprice = 0;
        int max_selection = 0;
        int min_selection = 0;
        string next_modset = "";

        string img_flag = "";

        string picture_image = "";

        int selectedQty = 0;
        int inpQty = 0;

        string modsoldout = "";
        string localmodsoldout = "";

        string disp_caption = "";

        string disp_price = "";
        bool isVisible = false;

        string fontcolor = "";
        string bgcolor = "";
        string fontfamily = "";
        string fontsize = "";
        string fontstyle = "";
        string kitchenName = "";

        public string Sid { get => sid; set => sid = value; }

        public string Psid { get => psid; set => psid = value; }
        
        public string Modifier_code { get => modifier_code; set => modifier_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Mod_type { get => mod_type; set => mod_type = value; }

        public string Price_type { get => price_type; set => price_type = value; }
        public decimal Sprice { get => sprice; set => sprice = value; }
        public int Max_selection { get => max_selection; set => max_selection = value; }
        public int Min_selection { get => min_selection; set => min_selection = value; }
        public string Next_modset { get => next_modset; set => next_modset = value; }        
        public string Img_flag { get => img_flag; set => img_flag = value; }
        public string Picture { get => picture_image; set => picture_image = value; }
        
        public string Disp_caption { get => disp_caption; set => disp_caption = value; }
        public string Disp_price { get => disp_price; set => disp_price = value; }
        public string ModSoldOut { get => modsoldout; set => modsoldout = value; }

        public string LocalModSoldOut { get => localmodsoldout; set => localmodsoldout = value; }
        public int SelectedQty { get => selectedQty; set { selectedQty = value;  IsVisible = (selectedQty > 0); OnPropertyChanged("SelectedQty"); } }
        public Boolean IsVisible { get => isVisible; set { isVisible = value; OnPropertyChanged("IsVisible"); } }

        public PsModset01 ModifierSet { get => modifierSet; set => modifierSet = value; }
        public int InpQty { get => inpQty; set => inpQty = value; }
        public string FontColor { get => fontcolor; set => fontcolor = value; }
        public string Bgcolor { get => bgcolor; set => bgcolor = value; }
        public string FontFamily { get => fontfamily; set => fontfamily = value; }
        public string FontSize { get => fontsize; set => fontsize = value; }
        public string FontStyle { get => fontstyle; set => fontstyle = value; }
        public string KitchenName { get => kitchenName; set => kitchenName = value; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            
            if (handler != null)
            {
                //Debug.WriteLine("OnPropertyChanged  " + name);
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

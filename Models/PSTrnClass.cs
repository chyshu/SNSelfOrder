using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PRPos.Data
{
    public enum OrderType
    {
        DINING = 1,
        TAKEWAY = 2,
        UBEREAT = 3,
        ABORT = -1,
    }
    [Serializable()]
    public class PSTrn01sClass:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string customerId = "";
        private string store_Code = "";
        private string pos_No = "";
        private string deal_No = "";
        private string accDate = "";
        private string tdate = "";
        private string clerk_no = "";
        private string deal_code = "";
        private decimal dis_amt = 0;
        private decimal min_amt = 0;
        private decimal over_amt = 0;
        private decimal mms_amt = 0;
        private decimal tot_amt = 0;
        private decimal tax_amt = 0;
        private decimal ntax_amt = 0;
        private decimal ztax_amt = 0;
        private decimal ht_amt = 0;
        private string card_no  = "";
        private string buss_no = "";
        private string uld_yn = "";
        private string crt_no = "";
        private string crt_date = "";
        private string close_yn = "";
        private string ref_no = "";
        private string cnl_no = "";
        private string cnl_time = "";
        private string service_no = "";
        private string opener_no = "";
        private string opentime = "";
        private string sendtime = "";
        private string closetime = "";
        private string org_deal_no = "";
        private string del_deal_no = "";
        private string order_type = "";
        private int person = 0;
        private string relation_no = "";
        private ObservableCollection<PSTrn02sClass> orderItems = null;
        private ObservableCollection<PSTrn03sClass> payments = null;
        private bool isSelected = false;
        public PSTrn01sClass()
        {
            orderItems = new ObservableCollection<PSTrn02sClass>();
            payments = new ObservableCollection<PSTrn03sClass>();
        }

        public string CustomerId { get => customerId; set => customerId = value; }
        public string Store_Code { get => store_Code; set => store_Code = value; }
        public string Pos_No { get => pos_No; set => pos_No = value; }
        public string Deal_No { get => deal_No; set => deal_No = value; }
        public string AccDate { get => accDate; set => accDate = value; }
        public string Tdate { get => tdate; set => tdate = value; }
        public string Clerk_no { get => clerk_no; set => clerk_no = value; }
        public string Deal_code { get => deal_code; set => deal_code = value; }
        public decimal Dis_amt { get => dis_amt; set => dis_amt = value; }
        public decimal Min_amt { get => min_amt; set => min_amt = value; }
        public decimal Over_amt { get => over_amt; set => over_amt = value; }
        public decimal Mms_amt { get => mms_amt; set => mms_amt = value; }
        public decimal Tot_amt { get => tot_amt; set => tot_amt = value; }
        public decimal Tax_amt { get => tax_amt; set => tax_amt = value; }
        public decimal Ntax_amt { get => ntax_amt; set => ntax_amt = value; }
        public decimal Ztax_amt { get => ztax_amt; set => ztax_amt = value; }
        public decimal Ht_amt { get => ht_amt; set => ht_amt = value; }
        public string Card_no { get => card_no; set => card_no = value; }
        public string Buss_no { get => buss_no; set => buss_no = value; }
        public string Uld_yn { get => uld_yn; set => uld_yn = value; }
        public string Crt_no { get => crt_no; set => crt_no = value; }
        public string Crt_date { get => crt_date; set => crt_date = value; }
        public string Close_yn { get => close_yn; set => close_yn = value; }
        public string Ref_no { get => ref_no; set => ref_no = value; }
        public string Cnl_no { get => cnl_no; set => cnl_no = value; }
        public string Cnl_time { get => cnl_time; set => cnl_time = value; }
        public string Service_no { get => service_no; set => service_no = value; }
        public string Opener_no { get => opener_no; set => opener_no = value; }
        public string Opentime { get => opentime; set => opentime = value; }
        public string Sendtime { get => sendtime; set => sendtime = value; }
        public string Closetime { get => closetime; set => closetime = value; }
        public string Org_deal_no { get => org_deal_no; set => org_deal_no = value; }
        public string Del_deal_no { get => del_deal_no; set => del_deal_no = value; }
        public string Order_type { get => order_type; set => order_type = value; }
        public int Person { get => person; set => person = value; }
        public ObservableCollection<PSTrn02sClass> OrderItems { get => orderItems; set => orderItems = value; }
        public ObservableCollection<PSTrn03sClass> Payments { get => payments; set => payments = value; }
        public bool ItemSelected { get => isSelected; set => isSelected = value;  }
        public string Relation_no { get => relation_no; set => relation_no = value; }

        protected void OnPropertyChanged( string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;            
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    [Serializable()]
    public class PSTrn02sClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string customerId = "";
        private string store_Code = "";
        private string pos_No = "";
        private string deal_No = "";
        private string accDate = "";
        private int item_No;
        private string item_Code = "";
        private string item_Name = "";
        private string kitchen_Memo = "";
        private string kitchen_Name = "";
        private string size_Code = "";
        private string item_Type = "";
        private decimal gst;
        private decimal goo_Price;
        private decimal sprice;
        private decimal ht_price;
        private int qty;
        private int calQty;
        private decimal tax_Amt;
        private decimal mis_Amt;
        private decimal dis_Amt;
        private decimal dis_Rate;
        private decimal amount;
        private decimal calAmount;
        private decimal ht_Amt;
        private decimal mms_mis ;
        private string mms_no = "";
        private string variety_Code = "";
        private string combo_Code = "";        
        private string modset_Code = "";
        private string variety_Caption = "";
        private string variety_Kitchen_name = "";                
        private string printer_Name = "";
        private string size_Caption = "";
        private string itemPicture = "";

        private ObservableCollection<PSTrn04sClass> modifiers = null;
        private PRPos.Data.FastKeyClass fastKey;
        public string CustomerId { get => customerId; set => customerId = value; }
        public string Store_Code { get => store_Code; set => store_Code = value; }
        public string Pos_No { get => pos_No; set => pos_No = value; }
        public string Deal_No { get => deal_No; set => deal_No = value; }
        public string AccDate { get => accDate; set => accDate = value; }
        public int Item_No { get => item_No; set => item_No = value; }
        public string Item_Code { get => item_Code; set { item_Code = value; OnPropertyChanged("Item_Code"); } }
        public string Item_Name { get => item_Name; set { item_Name = value; OnPropertyChanged("Item_Name"); } }        
        public string Kitchen_Memo { get => kitchen_Memo; set => kitchen_Memo = value; }
        public string Kitchen_Name { get => kitchen_Name; set => kitchen_Name = value; }
        public string Size_Code { get => size_Code; set { size_Code = value; OnPropertyChanged("Size_Code"); } }
        public string Item_Type { get => item_Type; set => item_Type = value; }
        public decimal GST { get => gst; set { gst = value; Tax_Amt = Amount * (gst / 100); OnPropertyChanged("GST"); } }
        public decimal Goo_Price { get => goo_Price; set { goo_Price = value; OnPropertyChanged("Goo_Price"); } }
        public decimal Sprice { get => sprice; set { sprice = value; Tax_Amt = Amount * (gst / 100); OnPropertyChanged("Sprice"); } }
        public decimal Ht_price { get => ht_price; set => ht_price = value; }
        public int Qty { get => qty; set { qty = value;  Tax_Amt = Amount * (gst / 100); OnPropertyChanged("Qty"); } }
        public decimal CalAmount { get => calAmount; set { calAmount = value; OnPropertyChanged("CalAmount"); } }
        public int CalQty { get => calQty; set { calQty = value; OnPropertyChanged("CalQty"); } }
        public decimal Tax_Amt { get => tax_Amt; set => tax_Amt = value; }
        public decimal Mis_Amt { get => mis_Amt; set => mis_Amt = value; }
        public decimal Dis_Amt { get => dis_Amt; set => dis_Amt = value; }
        public decimal Dis_Rate { get => dis_Rate; set => dis_Rate = value; }
        public decimal Amount { get => amount; set { amount = value; Tax_Amt = amount * (gst / 100); OnPropertyChanged("Amount"); } }
        public decimal Ht_Amt { get => ht_Amt; set => ht_Amt = value; }
        public decimal Mms_mis { get => mms_mis; set => mms_mis = value; }
        public string Mms_no { get => mms_no; set => mms_no = value; }
        public string Variety_Code { get => variety_Code; set { variety_Code = value; OnPropertyChanged("Variety_Code"); } }
        public string Combo_Code { get => combo_Code; set => combo_Code = value; }
        public string Modset_Code { get => modset_Code; set => modset_Code = value; }
        public string Variety_Caption { get => variety_Caption; set { variety_Caption = value; OnPropertyChanged("Variety_Caption"); } }
        public string Variety_Kitchen_name { get => variety_Kitchen_name; set => variety_Kitchen_name = value; }
        public string Printer_Name { get => printer_Name; set => printer_Name = value; }
        public string Size_Caption { get => size_Caption; set { size_Caption = value; OnPropertyChanged("Size_Caption"); } }

        public ObservableCollection<PSTrn04sClass> Modifiers
        {
            get => modifiers;
            set
            {
                modifiers = value;
                OnPropertyChanged("Modifiers");
            }
        }

        public string ItemPicture { get => itemPicture; set => itemPicture = value; }
        public FastKeyClass FastKey { get => fastKey; set => fastKey = value; }        

        public PSTrn02sClass()
        {
            modifiers = new ObservableCollection<PSTrn04sClass>();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    [Serializable()]
    public class PSTrn04sClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string customerId = "";
        private string store_Code = "";
        private string pos_No = "";
        private string deal_No = "";
        private string accDate = "";
        private int item_No;
        private string item_Code = "";
        private string variety_Code = "";
        private string modset_Code = "";
        private string modifier_Code = "";

        private string caption = "";
        private string caption_fn = "";
        private decimal sprice;
        private decimal amount;
        private int qty;
        private int calQty;
        private int inpQty;
        private decimal calSprice;
        private string kitchen_name = "";

        public string CustomerId { get => customerId; set => customerId = value; }
        public string Store_Code { get => store_Code; set => store_Code = value; }
        public string Pos_No { get => pos_No; set => pos_No = value; }
        public string Deal_No { get => deal_No; set => deal_No = value; }
        public string AccDate { get => accDate; set => accDate = value; }
        public int Item_No { get => item_No; set => item_No = value; }
        public string Item_Code { get => item_Code; set => item_Code = value; }
        public string Variety_Code { get => variety_Code; set => variety_Code = value; }
        public string Modset_Code { get => modset_Code; set => modset_Code = value; }
        public string Modifier_Code { get => modifier_Code; set => modifier_Code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public decimal Sprice { get => sprice; set { sprice = value; OnPropertyChanged("Sprice"); } }

        public decimal CalSprice { get => calSprice; set { calSprice = value; OnPropertyChanged("CalSprice"); } }
        public decimal Amount { get => amount; set { amount = value; OnPropertyChanged("Amount");  } }
        public int Qty { get => qty; set { qty = value; OnPropertyChanged("Qty"); } }
        public int InpQty { get => inpQty; set { inpQty = value; OnPropertyChanged("InpQty"); } }
        public int CalQty { get => calQty; set { calQty = value; OnPropertyChanged("CalQty"); } }

        public string Kitchen_Name { get => kitchen_name; set => kitchen_name = value; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
    [Serializable()]
    public class PSTrn03sClass : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private string customerId = "";
        private string store_Code = "";
        private string pos_No = "";
        private string deal_No = "";
        private string accDate = "";
        private int item_No;
        private string dc_code = "";
        private string ecp_type = "";
        private decimal ecp_amt;
        private decimal change_amt;
        private string epc_code = "";
        private string memo = "";
        private string ecp_name = "";
        private string ref_code1 = "";
        private string ref_code2 = "";

        public string CustomerId { get => customerId; set => customerId = value; }
        public string Store_Code { get => store_Code; set => store_Code = value; }
        public string Pos_No { get => pos_No; set => pos_No = value; }
        public string Deal_No { get => deal_No; set => deal_No = value; }
        public string AccDate { get => accDate; set => accDate = value; }
        public int Item_No { get => item_No; set => item_No = value; }
        public string Dc_code { get => dc_code; set => dc_code = value; }
        public string Ecp_type { get => ecp_type; set => ecp_type = value; }
        public decimal Ecp_amt { get => ecp_amt; set => ecp_amt = value; }
        public decimal Change_amt { get => change_amt; set => change_amt = value; }
        public string Epc_code { get => epc_code; set => epc_code = value; }
        public string Memo { get => memo; set => memo = value; }
        public string Ecp_name { get => ecp_name; set => ecp_name = value; }
        public string Ref_code1 { get => ref_code1; set => ref_code1 = value; }
        public string Ref_code2 { get => ref_code2; set => ref_code2 = value; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

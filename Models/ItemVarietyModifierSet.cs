using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRPos.Data
{
    [Serializable]
    public class ItemVarietyModifierSet : INotifyPropertyChanged
    {
        string itemCode = "";
        string varietyCode = "";
        string varietyCaption = "";
        
        string modsetCode = "";        
        string caption = "";
        string caption_fn = "";

        private decimal amount = 0;
        private decimal sprice = 0;
        private int qty = 0;
        string picture = "";
        public string Picture { get => picture; set => picture = value; }
        public decimal Amount
        {
            get => amount; set { amount = value; OnPropertyChanged("Amount"); }
        }
        public decimal Sprice
        {
            get => sprice; set { sprice = value; OnPropertyChanged("Sprice"); }
        }
        public int Qty
        {
            get => qty; set { qty = value; OnPropertyChanged("Qty"); }
        }
        public string VarietyCode
        {
            get => varietyCode; set { varietyCode = value; OnPropertyChanged("VarietyCode"); }
        }
        public string ItemCode
        {
            get => itemCode; set { itemCode = value; OnPropertyChanged("ItemCode"); }
        }
        public string ModsetCode
        {
            get => modsetCode; set { modsetCode = value; OnPropertyChanged("ModsetCode"); }
        }
        public string Caption
        {
            get => caption; set { caption = value; OnPropertyChanged("Caption"); }
        }
        public string Caption_fn
        {
            get => caption_fn; set { caption_fn = value; OnPropertyChanged("Caption_fn"); }
        }
        public string VarietyCaption { get => varietyCaption; set { varietyCaption = value; OnPropertyChanged("VarietyCaption"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            Debug.WriteLine("OnPropertyChanged ItemModifier ");
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

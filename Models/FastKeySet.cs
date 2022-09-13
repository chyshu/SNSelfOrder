using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PRPos.Data
{
    [Serializable()]
    public class FastkeySet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        int sid = 0;
        int pSid = 0;
        string pcode = "";
        string selected = "";
        string caption = "";
        string caption2 = "";
        string caption3 = "";
        string soldOut = "";        
        string priceLine = "";
        int width = 200;
        int height = 200;
        string display_yn = "";
        string default_yn = "";
        string fontcolor = "#FFFFFF";
        string bgColor = "#dddddd";
        string fontFamily = "";
        int fontSize = 12;
        string fontStyle = "";
        string picture = "";
        int seq = 0;
        decimal sprice = 0;
        decimal takeawayprice = 0;
        string upd_date = "";
        string str_upd_date = "";
        int textOffset = 60;
        string textBgColor = "Transparent";
        string text_display_yn = "N";
        string text2_display_yn = "N";
        string text3_display_yn = "N";
        string full_Image_yn = "N";
        ObservableCollection<FastKeyClass> fastkeyItems = new ObservableCollection<FastKeyClass>();
        public int PSid { get => pSid; set => pSid = value; }
        public int Sid { get => sid; set => sid = value; }
        public string PCode { get => pcode; set => pcode = value; }
        public string Selected { get => selected; set => selected = value; }
        public string Caption { get => caption; set => caption = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public string Display_yn { get => display_yn; set => display_yn = value; }
        public string Default_yn { get => default_yn; set => default_yn = value; }
        public string FontColor { get => fontcolor; set => fontcolor = value; }
        public string FontFamily { get => fontFamily; set => fontFamily = value; }
        public int FontSize { get => fontSize; set => fontSize = value; }
        public string FontStyle { get => fontStyle; set => fontStyle = value; }
        public string Picture { get => picture; set => picture = value; }
        public int Seq { get => seq; set => seq = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string BackColor { get => bgColor; set => bgColor = value; }
        public int TextOffset { get => textOffset; set => textOffset = value; }
        public string TextBgColor { get => textBgColor; set => textBgColor = value; }
        public string Text_display_yn { get => text_display_yn; set => text_display_yn = value; }
        public string Text2_display_yn { get => text2_display_yn; set => text2_display_yn = value; }
        public string Text3_display_yn { get => text3_display_yn; set => text3_display_yn = value; }
        public string Full_Image_yn { get => full_Image_yn; set => full_Image_yn = value; }
        public string Caption2 { get => caption2; set => caption2 = value; }
        public string Caption3 { get => caption3; set => caption3 = value; }
        public decimal Sprice { get => sprice; set => sprice = value; }
        public decimal Takeawayprice { get => takeawayprice; set => takeawayprice = value; }
        public string SoldOut { get => soldOut; set => soldOut = value; }        
        public string PriceLine { get => priceLine; set => priceLine = value; }

        public bool IsVisible { get { return ! SoldOut.Equals("Y"); } }
        public ObservableCollection<FastKeyClass> FastkeyItems
        {
            get { return fastkeyItems; }
            set { fastkeyItems = value; OnPropertyChanged("FastkeyItems"); }
        }

        public string Str_Upd_date { get => str_upd_date; set => str_upd_date = value; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            //Debug.WriteLine("OnPropertyChanged FastKeySet ");
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRPos.Data
{
    [Serializable()]
    public class FastKeyClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        int sid = 0;
        int pSid = 0;
        string pcode = "";
        string selected = "";
        string itemCode = "";
        string itemName = "";
        string caption = "";
        string caption2 = "";
        string caption3 = "";
        string description = "";
        string spicy = "";
        string soldOut = "";
        string strSoldOut = "";
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
        string imgUrl= "";
        int seq = 0;
        decimal sprice = 0;
        decimal takeawayprice = 0;
        string upd_date = "";
        string str_Upd_date = "";
        int textHeight = 60;
        string textBgColor = "Transparent";
        string text_display_yn = "N";
        string text2_display_yn = "N";
        string text3_display_yn = "N";
        string full_Image_yn = "N";
        string sizeCode = "";
        decimal get = 0;
        DataRow psitem;
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
        public string ButtonImgURI { get => imgUrl; set => imgUrl = value; }
        public int Seq { get => seq; set => seq = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string BackColor { get => bgColor; set => bgColor = value; }
        public int TextHeight { get => textHeight; set => textHeight = value; }
        public string TextBgColor { get => textBgColor; set => textBgColor = value; }
        public string Text_display_yn { get => text_display_yn; set => text_display_yn = value; }
        public string Text2_display_yn { get => text2_display_yn; set => text2_display_yn = value; }
        public string Text3_display_yn { get => text3_display_yn; set => text3_display_yn = value; }
        public string Full_Image_yn { get => full_Image_yn; set => full_Image_yn = value; }
        public string Caption2 { get => caption2; set => caption2 = value; }
        public string Caption3 { get => caption3; set => caption3 = value; }

        public string Description { get => description; set => description = value; }
        public decimal Sprice { get => sprice; set => sprice = value; }

        public decimal GST { get => get; set => get = value; }
        public decimal Takeawayprice { get => takeawayprice; set => takeawayprice = value; }
        //public string SoldOut { get => soldOut; set => soldOut = value; }
        public string PriceLine { get => priceLine; set => priceLine = value; }

        public bool IsVisible { get { return true; } }
        public string SoldOut
        {
            get { return soldOut; }
            set { soldOut = value; OnPropertyChanged("SoldOut"); }
        }
        public string StrSoldOut { get => strSoldOut; set { strSoldOut = value; OnPropertyChanged("StrSoldOut"); }  }
        public string Spicy { get => spicy; set => spicy = value; }
        public string SizeCode { get => sizeCode; set => sizeCode = value; }
        public string ItemName { get => itemName; set => itemName = value; }
        public DataRow PsItem { get => psitem; set => psitem = value; }
        public string ItemCode { get => itemCode; set => itemCode = value; }
        public string Str_Upd_date { get => str_Upd_date; set => str_Upd_date = value; }

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

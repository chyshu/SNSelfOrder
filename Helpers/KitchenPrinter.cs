using PRPos.Data;
using SNSelfOrder.Interfaces;
using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNSelfOrder.Helpers
{
    public class KitchenPrinter : ISelfOrderKitchenPrinter
    {
        void PrintTask(PSTrn01sClass currentdeal,int itemIndex, string _PrinterName)
        {
           
        }
        public int PrintingKitchenOrder(PSTrn01sClass currentdeal)
        {
            foreach (var printer in PRPosUtils.PosPrinters.Select(fld => new { fld.DeviceType, fld.DeviceName, fld.PrinterName, fld.IsDefault }).
                         Where(f => f.DeviceType == "Kitchen Printer" && f.PrinterName != ""))
            {
                if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => printer.DeviceName.Trim() == name.Trim()))
                {
                    for( int itemIndex = 0;itemIndex< currentdeal.OrderItems.Count;itemIndex++)                    
                    {
                        ThreadPrint printclass = new ThreadPrint(currentdeal, itemIndex, printer.DeviceName);
                        Thread t = new Thread(new ThreadStart(printclass.PrintProc));
                        t.Start();
                    }
                    
                }
            }
            return 0;
        }
        List<Models.PosPrinter> posPrinter;
        public  void SetPrinter(List<PosPrinter> printers)
        {
            posPrinter = printers;
        }
    }
    public class ThreadPrint
    {
        private PSTrn01sClass _currentdeal;
        private int _itemIndex;
        private string _printerName;
        public ThreadPrint(PSTrn01sClass currentdeal, int itemIndex, string PrinterName)
        {
            _currentdeal = currentdeal;
            _itemIndex = itemIndex;
            _printerName = PrinterName;
        }
        public void PrintProc()
        {
            var item = _currentdeal.OrderItems[_itemIndex];

            System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.PrinterSettings.PrinterName = _printerName;// printer.DeviceName;
            pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
            //Debug.WriteLine(pd.DefaultPageSettings.PaperSize);
            SizeF layoutSize = new SizeF(pd.DefaultPageSettings.PaperSize.Width-50  , pd.DefaultPageSettings.PaperSize.Height);
            
            pd.PrintPage += (s, e) =>
            {
                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                nfi.CurrencySymbol = "$";
                int CHARS_PERLINE = 30;
                int QTY_LENGTH = 4;
                int PRICE_LENGTH = 7;
                int ITEM_LENGTH = CHARS_PERLINE - QTY_LENGTH;
                //  Font stringFont = new Font("Courier New", 12);
                Font stringFont = new Font(PRPosUtils.KitchenOrderHeaderFontFamily, PRPosUtils.KitchenOrderHeaderFontSize);
                SizeF stringSize = new SizeF();
                Rectangle rect = e.PageBounds;
                float linePadding = 0.0f;
                float y = 0f;
                float x = 0f;
                string output = "";
                // foreach (var item in currentdeal.OrderItems)


                foreach (string str in PRPosUtils.KitchenOrderHeader)
                {
                    output = str.Replace("$$deal_no$$", _currentdeal.Deal_No);

                    stringFont = new Font(PRPosUtils.KitchenOrderItemFontFamily, PRPosUtils.KitchenOrderHeaderFontSize);

                    stringSize = e.Graphics.MeasureString(output, stringFont);
                    x = (rect.Width / 2) - (stringSize.Width / 2);

                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                    y += stringSize.Height + linePadding;
                }


                x = 0;
                output = item.Kitchen_Name;
                byte[] buffer = UTF8Encoding.UTF8.GetBytes(output);

                int pad = ITEM_LENGTH - buffer.Length;
                // output = output.PadRight(pad, ' ') + item.Qty.ToString().PadLeft(QTY_LENGTH, ' ');
                stringFont = new Font(PRPosUtils.KitchenOrderItemFontFamily, PRPosUtils.KitchenOrderItemFontSize);

                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width );                               
                SizeF stringSize2 = e.Graphics.MeasureString(item.Qty.ToString().PadLeft(QTY_LENGTH, ' ') , stringFont, (int)layoutSize.Width);
                

                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF( new PointF(x, y), stringSize) );

                y += stringSize.Height + linePadding;
                // e.Graphics.DrawString(item.Qty.ToString().PadLeft(QTY_LENGTH, ' ') , stringFont, System.Drawing.Brushes.Black, new PointF(layoutSize.Width, y- stringSize2.Height) );

                e.Graphics.DrawString(item.Qty.ToString().PadLeft(QTY_LENGTH, ' '), stringFont, System.Drawing.Brushes.Black, 
                    new RectangleF(new PointF(layoutSize.Width, y - stringSize2.Height), stringSize2));

                foreach (var mod in item.Modifiers)
                {
                    x = 10;
                    output = mod.Kitchen_Name;
                    buffer = UTF8Encoding.UTF8.GetBytes(output);

                    //pad = ITEM_LENGTH - buffer.Length;
                    //output = output.PadRight(pad, ' ') + mod.InpQty.ToString().PadLeft(QTY_LENGTH, ' ');
                    stringFont = new Font(PRPosUtils.KitchenOrderItemFontFamily, PRPosUtils.KitchenOrderItemFontSize);

                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width-10 );
                    stringSize2 = e.Graphics.MeasureString(mod.InpQty.ToString().PadLeft(QTY_LENGTH, ' '), stringFont, (int)layoutSize.Width);

                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), stringSize));

                    y += stringSize.Height + linePadding;
                    e.Graphics.DrawString(mod.InpQty.ToString().PadLeft(QTY_LENGTH, ' '), stringFont, System.Drawing.Brushes.Black, 
                        new RectangleF(new PointF(layoutSize.Width, y - stringSize2.Height), stringSize2));
                }

                output = DateTime.Parse(_currentdeal.Opentime).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat) + "  " + _currentdeal.Clerk_no;
                stringFont = new Font(PRPosUtils.KitchenOrderItemFontFamily, PRPosUtils.KitchenOrderItemFontSize);
                stringSize = e.Graphics.MeasureString(output, stringFont);
                x = (rect.Width / 2) - (stringSize.Width / 2);

                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                y += stringSize.Height + linePadding;

            };

            // pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.MyPrintDocument_PrintPage);
            pd.PrintController = new System.Drawing.Printing.StandardPrintController();
            pd.Print();
        }
    }
}

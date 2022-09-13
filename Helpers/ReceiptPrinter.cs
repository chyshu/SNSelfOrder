using PRPos.Data;
using SNSelfOrder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Diagnostics;

namespace SNSelfOrder.Helpers
{
    public class ReceiptPrinter : ISelfOrderPrinter
    {
        List<Models.PosPrinter> posPrinter;
        public int PrintingCardReceipt(string recepit)
        {
            foreach (var printer in posPrinter.Select(f => new { f.DeviceType, f.DeviceName, f.IsDefault, f.PrinterName }).Where(f => f.DeviceType == "Receipt Printer"))
            {
                if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => printer.DeviceName.Trim() == name.Trim()))
                {
                    System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();

                    pd.PrinterSettings.PrinterName = printer.DeviceName;
                    pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
                    pd.PrintPage += (s, e) =>
                    {
                        string[] lines = recepit.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        Font stringFont = new Font("Consolas", 9);
                        SizeF stringSize = new SizeF();
                        float y = 0f;
                        float linePadding = 0.0f;
                        foreach (string str in lines)
                        {
                            stringSize = e.Graphics.MeasureString(str, stringFont);
                            e.Graphics.DrawString(str, stringFont, System.Drawing.Brushes.Black, new PointF(0, y));
                            y += stringSize.Height + linePadding;
                        }
                    };

                    pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                    pd.Print();
                }
            }
            return 0;
        }

 
        private void printReceipt(PSTrn01sClass currentdeal, bool isReprint = false)
        {
            foreach (var printer in PRPosUtils.PosPrinters.Select(fld => new { fld.DeviceType, fld.DeviceName, fld.PrinterName, fld.IsDefault }).
                          Where(f => f.DeviceType == "Receipt Printer" && f.PrinterName != ""))
            {
                if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => printer.DeviceName.Trim() == name.Trim()))
                {
                    System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrinterSettings.PrinterName = printer.DeviceName;
                    pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();

                    SizeF layoutSize = new SizeF(pd.DefaultPageSettings.PaperSize.Width, pd.DefaultPageSettings.PaperSize.Height);

                    pd.PrintPage += (s, e) =>
                    {

                        NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                        nfi.CurrencySymbol = "$";
                        int CHARS_PERLINE = PRPosUtils.ReceiptCharacters;
                        int QTY_LENGTH = PRPosUtils.ReceiptItemQtyCharacters;
                        int PRICE_LENGTH = PRPosUtils.ReceiptPriceCharacters + PRPosUtils.ReceiptPriceDigitals + 1;
                        int AMOUNT_LENGTH = PRPosUtils.ReceiptPriceCharacters + PRPosUtils.ReceiptPriceDigitals + 1;
                        int ITEM_LENGTH = CHARS_PERLINE - QTY_LENGTH - AMOUNT_LENGTH - PRICE_LENGTH;

                        Font stringFont = new Font(PRPosUtils.ReceiptItemItemFontFamily, PRPosUtils.ReceiptItemFontSize);
                        Font stringFont2 = new Font(PRPosUtils.ReceiptItemItemFontFamily, PRPosUtils.ReceiptItemFontSize + 2);
                        
                        SizeF QtySize = e.Graphics.MeasureString( PRPosUtils.repeatStr("9", PRPosUtils.ReceiptItemQtyCharacters), stringFont);
                        SizeF PriceSize = e.Graphics.MeasureString("@"+PRPosUtils.repeatStr("9", PRPosUtils.ReceiptPriceCharacters) + "." +
                                                                   PRPosUtils.repeatStr("9", PRPosUtils.ReceiptPriceDigitals), stringFont);
                        SizeF AmountSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("9", PRPosUtils.ReceiptPriceCharacters) + "." +
                                                                   PRPosUtils.repeatStr("9", PRPosUtils.ReceiptPriceDigitals), stringFont);
                        int LineHeight = QtySize.ToSize().Height;
                        //  Debug.WriteLine(layoutSize+" "+ QtySize.ToSize() + " "+ PriceSize.ToSize());
                        int qtyWidth = QtySize.ToSize().Width;
                        int priceWidth = PriceSize.ToSize().Width;
                        int amountWidth = AmountSize.ToSize().Width;
                        int itemWidth = (int)layoutSize.Width - qtyWidth - priceWidth- amountWidth;

                        SizeF stringSize = new SizeF();
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;


                        Rectangle rect = e.PageBounds;
                        float linePadding = 0.0f;
                        float y = 0f;
                        float x = 0f;
                        string output = "";

                        if (currentdeal.Deal_code == "R")
                        {

                            output = "Refund";

                            stringSize = e.Graphics.MeasureString(output, stringFont2, (int)layoutSize.Width, stringFormat);
                            e.Graphics.FillRectangle(System.Drawing.Brushes.Black, x, y, rect.Width, stringSize.Height);
                            e.Graphics.DrawString(output, stringFont2, System.Drawing.Brushes.White,
                                new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }

                        output = PRPosDB.ReadString("recipt_title").Trim();

                        stringSize = e.Graphics.MeasureString(output, stringFont2, (int)layoutSize.Width, stringFormat);
                        x = 0; // (rect.Width / 2) - (stringSize.Width / 2);
                        e.Graphics.DrawString(output, stringFont2, System.Drawing.Brushes.Black,
                            new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                        y += stringSize.Height + linePadding;
                        // e.Graphics.DrawRectangle(new Pen(System.Drawing.Brushes.Black), x, y, rect.Width, stringSize.Height);
                        

                        if (isReprint)
                        {
                           
                            output = "RePrint";
                            stringSize = e.Graphics.MeasureString(output, stringFont2, (int)layoutSize.Width, stringFormat);                            
                            // e.Graphics.FillRectangle(System.Drawing.Brushes.Black, rect.Width - stringSize.Width, y, stringSize.Width, stringSize.Height);
                            e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, 
                                new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                     
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Near;
                        // stringFont = new Font("Consolas", 8);

                        if (!PRPosDB.ReadString("company_name").Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(PRPosDB.ReadString("company_name"), stringFont, (int)layoutSize.Width);
                            // x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(PRPosDB.ReadString("company_name"), stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        if (!PRPosDB.ReadString("buss_no").Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(PRPosDB.ReadString("buss_no"), stringFont, (int)layoutSize.Width);
                            // x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(PRPosDB.ReadString("buss_no"), stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        if (!PRPosDB.ReadString("store_name").Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(PRPosDB.ReadString("store_name"), stringFont, (int)layoutSize.Width);
                            // x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(PRPosDB.ReadString("store_name"), stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        if (!PRPosDB.ReadString("store_phone").Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(PRPosDB.ReadString("store_phone"), stringFont, (int)layoutSize.Width);
                            //x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(PRPosDB.ReadString("store_phone"), stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        string addr1 = PRPosDB.ReadString("store_address_line1");
                        if (!addr1.Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(addr1, stringFont, (int)layoutSize.Width);
                            // x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(addr1, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        addr1 = PRPosDB.ReadString("store_address_line2");
                        if (!addr1.Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(addr1, stringFont, (int)layoutSize.Width);
                            //x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(addr1, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }
                        addr1 = PRPosDB.ReadString("store_address_line3");
                        if (!addr1.Equals(""))
                        {
                            stringSize = e.Graphics.MeasureString(addr1, stringFont, (int)layoutSize.Width);
                            // x = (rect.Width / 2) - (stringSize.Width / 2);
                            e.Graphics.DrawString(addr1, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(rect.Width, stringSize.Height)), stringFormat);
                            y += stringSize.Height + linePadding;
                        }


                        stringFormat.Alignment = StringAlignment.Near;
                        stringFormat.LineAlignment = StringAlignment.Near;
                        stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", CHARS_PERLINE), stringFont);
                        x = 0;

                        e.Graphics.DrawString(PRPosUtils.repeatStr("=", CHARS_PERLINE), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                        y += stringSize.Height + linePadding;


                        // if (currentdeal.Deal_code == "S")
                        {
                            if (PRPosUtils.Ask_OrderType == "Y")
                            {
                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Near;
                                output = currentdeal.Order_type.Equals("1")?"Dine in": currentdeal.Order_type.Equals("2") ? "Takeaway" : currentdeal.Order_type.Equals("3") ? "Ubereat" : "Dine in";
                                stringFont2 = new Font(PRPosUtils.ReceiptItemItemFontFamily, PRPosUtils.ReceiptItemFontSize + 2);
                                stringSize = e.Graphics.MeasureString(output, stringFont2);
                                e.Graphics.FillRectangle(System.Drawing.Brushes.Black, rect.Width - stringSize.Width, y, stringSize.Width, stringSize.Height);
                                e.Graphics.DrawString(output, stringFont2, System.Drawing.Brushes.White, new RectangleF(new PointF(rect.Width - stringSize.Width, y), new SizeF(stringSize.Width, stringSize.Height)));
                            }
                        }
                        /* else
                         {
                             stringFormat.Alignment = StringAlignment.Center;
                             stringFormat.LineAlignment = StringAlignment.Near;
                             output = "REFUND";
                              stringFont2 = new Font(PRPosUtils.ReceiptItemItemFontFamily, PRPosUtils.ReceiptItemFontSize + 2);
                             stringSize = e.Graphics.MeasureString(output, stringFont2);
                             e.Graphics.FillRectangle(System.Drawing.Brushes.Black, rect.Width - stringSize.Width, y, stringSize.Width, stringSize.Height);
                             e.Graphics.DrawString(output, stringFont2, System.Drawing.Brushes.White, new RectangleF(new PointF(rect.Width - stringSize.Width, y), new SizeF(stringSize.Width, stringSize.Height)));
                         }*/

                        stringFormat.Alignment = StringAlignment.Near;
                        stringFormat.LineAlignment = StringAlignment.Near;
                        string[] lines = new string[]
                               {
                         // "Acc. Date :" + currentdeal.AccDate,
                          "Txn. Date :" + currentdeal.Tdate,
                          "No   :"+ currentdeal.Pos_No+" "+currentdeal.Deal_No
                            };
                        foreach (string line in lines)
                        {
                            stringSize = e.Graphics.MeasureString(line, stringFont, (int)layoutSize.Width);
                            e.Graphics.DrawString(line, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(stringSize.Width, stringSize.Height)));
                            y += stringSize.Height + linePadding;

                        }

                        if ((PRPosUtils.Ask_Table_Number != "0") || (PRPosUtils.Ask_Covers != "0"))
                        {
                            if (PRPosUtils.Ask_Table_Number != "0")
                            {
                                if (currentdeal.Order_type == "1")
                                    output = "Table Number:" + currentdeal.Ref_no;
                                else
                                    output = "Buzzer Number:" + currentdeal.Ref_no;
                                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width/2);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(stringSize.Width, stringSize.Height)));
                                x += (int)layoutSize.Width / 2;
                            }
                            if (PRPosUtils.Ask_Covers != "0")
                            {
                                if (currentdeal.Order_type == "1")
                                    output = "Cover:" + currentdeal.Person;
                                else
                                    output = "Cover:" + currentdeal.Person;
                                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width / 2);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(stringSize.Width, stringSize.Height)));
                                x += (int)layoutSize.Width / 2;
                            }
                            y += stringSize.Height + linePadding;
                            x = 0;
                        }
                        
                        output = "";

                        foreach (var item in currentdeal.OrderItems)
                        {
                            output = item.Item_Name;
                            stringFormat.Alignment = StringAlignment.Near;
                            stringFormat.LineAlignment = StringAlignment.Near;
                            stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                            e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                new SizeF(itemWidth, stringSize.Height)), stringFormat);
                            // e.Graphics.DrawRectangle(new Pen(System.Drawing.Brushes.Black),x, y, stringSize.Width, stringSize.Height );
                            stringFormat.Alignment = StringAlignment.Far;

                            SizeF _qtysize = e.Graphics.MeasureString(item.Qty.ToString("#0")+"x", stringFont, qtyWidth, stringFormat);

                            e.Graphics.DrawString(item.Qty.ToString("#0") + "x", stringFont, System.Drawing.Brushes.Black,
                                new RectangleF(new PointF(itemWidth, y), new SizeF(qtyWidth, _qtysize.Height)), stringFormat);

                            // e.Graphics.DrawRectangle(new Pen(System.Drawing.Brushes.Black), itemWidth, y, _qtysize.Width, _qtysize.Height);

                            SizeF _spricesize = e.Graphics.MeasureString("@"+item.Sprice.ToString("#0.00") , stringFont, priceWidth, stringFormat);

                            e.Graphics.DrawString("@" + item.Sprice.ToString("#0.00") , stringFont, System.Drawing.Brushes.Black,
                                new RectangleF(new PointF(itemWidth+ qtyWidth,  y), new SizeF(priceWidth, _spricesize.Height)), stringFormat);


                            SizeF _amountsize = e.Graphics.MeasureString( (item.Sprice* item.Qty).ToString("#0.00"), stringFont, amountWidth, stringFormat);

                            e.Graphics.DrawString((item.Sprice * item.Qty).ToString("#0.00"), stringFont, System.Drawing.Brushes.Black,
                                new RectangleF(new PointF(itemWidth + qtyWidth+priceWidth, y), new SizeF(amountWidth, _amountsize.Height)), stringFormat);
                            // e.Graphics.DrawRectangle(new Pen(System.Drawing.Brushes.Black), itemWidth + qtyWidth, y, _pricesize.Width, _pricesize.Height);
                            y += stringSize.Height + linePadding;
                            /*
                            string[] token = item_name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            output = "";
                            string comma = "";
                            List<string> Line = new List<string>();
                            foreach (string str in token)
                            {
                                if ((output.Length + str.Length + 1) >= ITEM_LENGTH)
                                {
                                    output += comma + str;
                                    Line.Add(output);
                                    output = "";
                                    comma = "";
                                }
                                else
                                {
                                    output += comma + str;
                                    comma = " ";
                                }
                            }
                            if (output != "") Line.Add(output);
                            for (int i = 0; i < Line.Count; i++)
                            {
                                output = Line[i];
                                if (i == 0)
                                    output = output.PadRight(ITEM_LENGTH, ' ') + item.Qty.ToString("#0").PadLeft(QTY_LENGTH, ' ') + (item.Sprice * item.Qty).ToString("##0.00").PadLeft(PRICE_LENGTH, ' ');
                                else
                                    output = output.PadRight(ITEM_LENGTH, ' ');
                                stringSize = e.Graphics.MeasureString(output, stringFont);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;
                            }
                            */
                            foreach (var m in item.Modifiers)
                            {
                                output = m.Caption;
                                stringFormat.Alignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth - 10, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x + 10, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;

                                _qtysize = e.Graphics.MeasureString(m.Qty.ToString("#0")+"x", stringFont, qtyWidth, stringFormat);
                                e.Graphics.DrawString(m.Qty.ToString("#0") + "x", stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth, y), new SizeF(qtyWidth, _qtysize.Height)), stringFormat);


                                _spricesize = e.Graphics.MeasureString("@"+m.Sprice.ToString("#0.00"), stringFont, priceWidth, stringFormat);

                                e.Graphics.DrawString("@"+m.Sprice.ToString("#0.00"), stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _spricesize.Height)), stringFormat);

                                _amountsize = e.Graphics.MeasureString((m.Sprice * m.Qty).ToString("#0.00"), stringFont, amountWidth, stringFormat);

                                e.Graphics.DrawString((m.Sprice * m.Qty).ToString("#0.00"), stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth + priceWidth, y), new SizeF(amountWidth, _amountsize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;

                                /*
                                token = ModName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                output = "";
                                comma = "";
                                Line = new List<string>();
                                foreach (string str in token)
                                {
                                    if ((output.Length + str.Length + 1) >= (ITEM_LENGTH - 2))
                                    {
                                        output += comma + str;
                                        Line.Add(output);
                                        output = "";
                                        comma = "";
                                    }
                                    else
                                    {
                                        output += comma + str;
                                        comma = " ";
                                    }
                                }
                                if (output != "") Line.Add(output);

                                for (int i = 0; i < Line.Count; i++)
                                {
                                    output = "  " + Line[i];
                                    if (i == 0)
                                    {
                                        output = output.PadRight(ITEM_LENGTH, ' ') + m.Qty.ToString("#0").PadLeft(QTY_LENGTH, ' ') + (m.Sprice * m.Qty).ToString("##0.00").PadLeft(PRICE_LENGTH, ' ');
                                    }
                                    else
                                        output = output.PadRight(ITEM_LENGTH, ' ');
                                    stringSize = e.Graphics.MeasureString(output, stringFont);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                    y += stringSize.Height + linePadding;
                                }*/
                            }
                        }
                        e.Graphics.DrawString(PRPosUtils.repeatStr("=", CHARS_PERLINE), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                        y += stringSize.Height + linePadding;

                        output = "Subtotal"; // .PadRight(ITEM_LENGTH, ' ') + string.Format(nfi, "{0:C}", currentdeal.Tot_amt).PadLeft(QTY_LENGTH + PRICE_LENGTH, ' ');
                        stringFormat.Alignment = StringAlignment.Near;
                        stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                        stringFormat.Alignment = StringAlignment.Far;
                        SizeF __amountSize = e.Graphics.MeasureString(string.Format(nfi, "{0:C}", currentdeal.Tot_amt), stringFont, amountWidth, stringFormat);
                        e.Graphics.DrawString(string.Format(nfi, "{0:C}", currentdeal.Tot_amt), stringFont, System.Drawing.Brushes.Black,
                            new RectangleF(new PointF(itemWidth + qtyWidth+ priceWidth, y), new SizeF(amountWidth, __amountSize.Height)), stringFormat);

                        y += stringSize.Height + linePadding;

                        output = "TOTAL for " + currentdeal.OrderItems.Count + " ITEMS";
                        stringFormat.Alignment = StringAlignment.Near;
                        stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                        // + string.Format(nfi, "{0:C}", currentdeal.Tot_amt).PadLeft(CHARS_PERLINE - 22, ' ');
                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                        stringFormat.Alignment = StringAlignment.Far;
                        __amountSize = e.Graphics.MeasureString(string.Format(nfi, "{0:C}", currentdeal.Tot_amt), stringFont, amountWidth, stringFormat);
                        e.Graphics.DrawString(string.Format(nfi, "{0:C}", currentdeal.Tot_amt), stringFont, System.Drawing.Brushes.Black,
                            new RectangleF(new PointF(itemWidth + qtyWidth + priceWidth, y), new SizeF(amountWidth, __amountSize.Height)), stringFormat);


                        y += stringSize.Height + linePadding;
                        y += stringSize.Height + linePadding;
                        foreach (var payment in currentdeal.Payments)
                        {
                            if (payment.Ecp_type.Equals("EFTPOS"))
                            {

                                //output = "Credit Card";//.PadRight(CHARS_PERLINE - PRICE_LENGTH - QTY_LENGTH, ' ') + string.Format(nfi, "{0:C}", payment.Ecp_amt).PadLeft(PRICE_LENGTH + QTY_LENGTH, ' ');
                                output = payment.Ecp_name;
                                stringFormat.Alignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, 
                                    new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                __amountSize = e.Graphics.MeasureString(string.Format(nfi, "{0:C}", payment.Ecp_amt), stringFont, amountWidth, stringFormat);
                                e.Graphics.DrawString(string.Format(nfi, "{0:C}", payment.Ecp_amt), stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth + priceWidth, y), new SizeF(amountWidth, __amountSize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;
                            }
                            else
                            {
                                output = payment.Ecp_name;//.PadRight(CHARS_PERLINE - PRICE_LENGTH - QTY_LENGTH, ' ') + string.Format(nfi, "{0:C}", payment.Ecp_amt).PadLeft(PRICE_LENGTH + QTY_LENGTH, ' ');
                                stringFormat.Alignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                __amountSize = e.Graphics.MeasureString(string.Format(nfi, "{0:C}", payment.Ecp_amt), stringFont, amountWidth, stringFormat);
                                e.Graphics.DrawString(string.Format(nfi, "{0:C}", payment.Ecp_amt), stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth + priceWidth, y), new SizeF(amountWidth, __amountSize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;
                            }
                        }
                        if (currentdeal.Deal_code == "R")
                            output = "REFUND  INCLUDES GST:";
                        else
                            output = "TRANSCATION INCLUDES GST:";
                        
                            
                        stringFormat.Alignment = StringAlignment.Near;
                        stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);
                        //.PadRight(CHARS_PERLINE - PRICE_LENGTH - QTY_LENGTH, ' ') + string.Format(nfi, "{0:C}", currentdeal.Tax_amt).PadLeft(PRICE_LENGTH + QTY_LENGTH, ' ');

                        stringFormat.Alignment = StringAlignment.Far;
                        __amountSize = e.Graphics.MeasureString(string.Format(nfi, "{0:C}", currentdeal.Tax_amt), stringFont, amountWidth, stringFormat);
                        e.Graphics.DrawString(string.Format(nfi, "{0:C}", currentdeal.Tax_amt), stringFont, System.Drawing.Brushes.Black,
                            new RectangleF(new PointF(itemWidth + qtyWidth + priceWidth, y), new SizeF(amountWidth, __amountSize.Height)), stringFormat);

                        y += stringSize.Height + linePadding;
                    };
                    // pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.MyPrintDocument_PrintPage);
                    pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                    pd.Print();
                }
            }
        }
        public int PrintingReceipt(PSTrn01sClass currentdeal)
        {
            printReceipt(currentdeal,false);
            return 0;
        }

        public int PrintingReceipt(PSTrn01sClass currentdeal, bool Reprint = false)
        {
            printReceipt(currentdeal, Reprint);
            return 0;
        }

        public void SetPrinter(List<Models.PosPrinter> printers)
        {
            posPrinter = printers;
        }

    }
}

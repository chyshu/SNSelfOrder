using PRPos.Data;
using SNSelfOrder.Interfaces;
using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Helpers
{
    public class LabelPrinter : ISelfOrderLabelPrinter
    {
        List<Models.PosPrinter> posPrinter;

        public int PrintingLabel(PSTrn01sClass currentdeal, decimal offSetX = 0, decimal offSetY = 0)
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            nfi.CurrencySymbol = "$";
            DateTime PrintTime = DateTime.Now;
            int QTY = 0;
            int LinesPerLabel = 5;

            decimal overallQTY = 0;
            int maxLineCharLength = 20;
            foreach (var item in currentdeal.OrderItems)
            {
                if (!(item.Item_Code == "Plastic Bag" || item.Item_Name == "Plastic Bag" || item.Item_Name.Contains("Surcharge") || item.Item_Name.Contains("surcharge")))
                    overallQTY += item.Qty;
            }
            foreach (var item in currentdeal.OrderItems)
            {
                //if ((item.Item_Code != "Plastic Bag") && (item.Item_Name != "Plastic Bag"))
                int topy = 10 + Convert.ToInt16(offSetY);
                int topx = 20 + Convert.ToInt16(offSetX);
       
                if (!(item.Item_Code == "Plastic Bag" || item.Item_Name == "Plastic Bag" || item.Item_Name.Contains("Surcharge") || item.Item_Name.Contains("surcharge")))
                {
                    // one cup last one label
                    foreach (var printer in posPrinter.Select(f => new { f.DeviceType, f.DeviceName, f.IsDefault, f.PrinterName }).Where(f => f.DeviceType == "Label Printer" && f.PrinterName == item.Printer_Name))
                    {
                        
                        for (int q = 1; q <= item.Qty; q++)
                        {
                            StringBuilder itemLabel = new StringBuilder();
                            QTY += 1;
                            
                            int linecnt = 0;

                            string itemName = String.IsNullOrEmpty(item.Variety_Kitchen_name) ? item.Kitchen_Name : item.Variety_Kitchen_name;
                            var longWord = itemName.SplitInParts(maxLineCharLength);
                            
                            foreach (string word in longWord)
                            {
                                if (linecnt == 0)
                                {
                                    itemLabel = new StringBuilder();
                                    itemLabel.AppendLine("^XA");
                                    itemLabel.AppendLine("^FO" + topx.ToString() + "," + topy.ToString() + "^AQN,11,7^FD" + currentdeal.Tdate + "^FS");
                                    itemLabel.Append("^FO" + topx.ToString() + "," + topy.ToString() + "^ASN,11,7^FB280,,,R,^FD" + " #" + currentdeal.Deal_No + "^FS");
                                }
                                topy += 28;
                                linecnt += 1;
                                itemLabel.AppendLine("^FO" + topx.ToString() + "," + topy.ToString() + "^ARN,11,7^FD" + word + "^FS");
                                if (linecnt== LinesPerLabel)
                                {
                                    itemLabel.AppendLine("^FO260,160^AQN,11,7^FD" + QTY + "/" + overallQTY + "^FS");
                                    itemLabel.AppendLine("^XZ");
                                    RawPrinterHelper.SendStringToPrinter(printer.DeviceName, itemLabel.ToString());
                                    linecnt = 0;
                                    topy = 10 + Convert.ToInt16(offSetY);
                                }
                            }
                            
                            #region Modifiers
                            foreach (var modifier in item.Modifiers)
                            {
                                int modQty = modifier.Qty;
                                string nameToSplit = modifier.Caption;
                                string partialTag = "*";

                                if (modifier.InpQty > 1)
                                {
                                    nameToSplit = modifier.Caption + " x" + modifier.InpQty;
                                }
                                var splitName = nameToSplit.SplitInParts(maxLineCharLength);

                                foreach (string partName in splitName)
                                {
                                    if (linecnt == 0)
                                    {
                                        itemLabel = new StringBuilder();
                                        itemLabel.AppendLine("^XA");
                                        itemLabel.AppendLine("^FO" + topx.ToString() + "," + topy.ToString() + "^AQN,11,7^FD" + currentdeal.Tdate + "^FS");
                                        itemLabel.Append("^FO" + topx.ToString() + "," + topy.ToString() + "^ASN,11,7^FB280,,,R,^FD" + " #" + currentdeal.Deal_No + "^FS");
                                    }

                                    topy += 27;
                                    itemLabel.Append("^FO" + topx.ToString() + "," + topy.ToString() + "^A0N,27,28^FD" +  partialTag   + partName + "^FS");
                                    partialTag = "-";
                                    linecnt += 1;
                                    if (linecnt == LinesPerLabel)
                                    {
                                        itemLabel.AppendLine("^FO260,160^AQN,11,7^FD" + QTY + "/" + overallQTY + "^FS");
                                        itemLabel.AppendLine("^XZ");
                                        RawPrinterHelper.SendStringToPrinter(printer.DeviceName, itemLabel.ToString());
                                        linecnt = 0;
                                        topy = 10 + Convert.ToInt16(offSetY);
                                    }
                                }                                
                            }
                            if (linecnt > 0)
                            {
                                itemLabel.AppendLine("^FO260,160^AQN,11,7^FD" + QTY + "/" + overallQTY + "^FS");
                                itemLabel.AppendLine("^XZ");
                                topy = 10 + Convert.ToInt16(offSetY);
                                RawPrinterHelper.SendStringToPrinter(printer.DeviceName, itemLabel.ToString());
                            }
                            
                            #endregion


                        }

                    }
                }
            }
                                   
            return 0;
        }

       
        public void SetPrinter(List<Models.PosPrinter> printer)
        {
            this.posPrinter = printer;
        }
    }
}
